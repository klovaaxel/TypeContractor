using System.Reflection;
using TypeContractor.Helpers;
using TypeContractor.Output;

namespace TypeContractor.TypeScript;

public class TypeScriptConverter(TypeContractorConfiguration configuration, MetadataLoadContext metadataLoadContext)
{
	public Dictionary<Type, OutputType> CustomMappedTypes { get; } = [];

	public OutputType Convert(ContractedType contractedType)
	{
		ArgumentNullException.ThrowIfNull(contractedType);
		return Convert(contractedType.Type, contractedType);
	}

	public OutputType Convert(Type type, ContractedType? contractedType = null)
	{
		ArgumentNullException.ThrowIfNull(type);

		var name = type.Name.Split('`').First();

		return new(
			name,
			type.FullName!,
			CasingHelpers.ToCasing(name.Replace("_", ""), configuration.Casing),
			contractedType ?? ContractedType.FromName(type.FullName!, type, configuration),
			type.IsEnum,
			type.IsEnum ? null : GetProperties(type).Distinct().ToList(),
			type.IsEnum ? GetEnumProperties(type) : null
		);
	}

	private List<OutputEnumMember> GetEnumProperties(Type type)
	{
		var matchAssembly = metadataLoadContext.LoadFromAssemblyName(type.Assembly.FullName!);
		var matchedEnumType = matchAssembly.GetType(type.FullName!)!;

		var underlyingValues = matchedEnumType.GetEnumValuesAsUnderlyingType();

		return matchedEnumType
			.GetEnumNames()
			.Select((name, idx) =>
			{
				var member = matchedEnumType.GetMember(name);
				var obsolete = member.FirstOrDefault()?.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
				var obsoleteInfo = obsolete is not null ? new ObsoleteInfo((string?)obsolete.ConstructorArguments.FirstOrDefault().Value) : null;
				return new OutputEnumMember(name, name, underlyingValues.GetValue(idx)!, obsoleteInfo);
			})
			.ToList();
	}

	private List<OutputProperty> GetProperties(Type type)
	{
		var outputProperties = new List<OutputProperty>();

		// Find all properties
		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var genericProperties = new List<PropertyInfo>();

		if (type.IsGenericType)
		{
			var genericTypeDefinition = type.GetGenericTypeDefinition();
			genericProperties = [.. genericTypeDefinition.GetProperties().Where(x => !properties.Any(y => y.PropertyType == x.PropertyType))];
		}

		// Evaluate type of property
		foreach (var property in properties)
		{
			// Need to have a getter
			if (!property.CanRead) continue;

			// Getter has to be public
			var getter = property.GetGetMethod(false);
			if (getter is null) continue;

			// Check if we have a setter
			var setter = property.GetSetMethod(false);
			var isReadonly = !property.CanWrite || setter is null;

			var destinationName = GetDestinationName(property.Name);
			var destinationType = GetDestinationType(property.PropertyType, property.CustomAttributes, isReadonly, TypeChecks.IsNullable(property.PropertyType));
			var outputProperty = new OutputProperty(property.Name, property.PropertyType, destinationType.InnerType, destinationName, destinationType.TypeName, destinationType.ImportType, destinationType.IsBuiltin, destinationType.IsArray, TypeChecks.IsNullable(property), destinationType.IsReadonly);

			var obsolete = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
			outputProperty.Obsolete = obsolete is not null ? new ObsoleteInfo((string?)obsolete.ConstructorArguments.FirstOrDefault().Value) : null;

			outputProperty.GenericType = genericProperties.Find(x => x.Name == property.Name)?.PropertyType;

			outputProperty.GenericTypeArguments = property.PropertyType.GenericTypeArguments.Length > 0 && outputProperty.GenericType is null
				? property.PropertyType.GenericTypeArguments
					.Select(x => GetDestinationType(x, x.CustomAttributes, false, TypeChecks.IsNullable(x)))
				: null;


			outputProperties.Add(outputProperty);
		}

		// Look at base classes
		if (type.BaseType is not null)
			outputProperties.AddRange(GetProperties(type.BaseType));

		// Look at interfaces
		foreach (var iface in type.GetInterfaces())
			outputProperties.AddRange(GetProperties(iface));

		return outputProperties;
	}

	public static string GetDestinationName(string name) => name.ToTypeScriptName();

	public DestinationType GetDestinationType(in Type sourceType, IEnumerable<CustomAttributeData> customAttributes, bool isReadonly, bool isNullable)
	{
		if (configuration.TypeMaps.TryGetValue(sourceType.FullName!, out var destType))
			return new DestinationType(destType.Replace("[]", string.Empty), sourceType.FullName, true, destType.Contains("[]"), isReadonly, isNullable || TypeChecks.IsNullable(sourceType), null);

		if (CustomMappedTypes.TryGetValue(sourceType, out var customType))
			return new DestinationType(customType.Name, customType.FullName, false, false, isReadonly, TypeChecks.IsNullable(sourceType), null);

		if (TypeChecks.ImplementsIDictionary(sourceType))
		{
			var keyType = GetDestinationType(TypeChecks.GetGenericType(sourceType, 0), customAttributes, isReadonly, false);
			var valueType = TypeChecks.GetGenericType(sourceType, 1);
			var valueDestinationType = GetDestinationType(valueType, customAttributes, isReadonly, TypeChecks.IsNullable(valueType));

			var isBuiltin = keyType.IsBuiltin && valueDestinationType.IsBuiltin;

			return new DestinationType($"{{ [key: {keyType.TypeName}]: {valueDestinationType.FullTypeName} }}", valueDestinationType.FullName, isBuiltin, false, isReadonly, valueDestinationType.IsNullable, valueType, valueDestinationType.ImportType);
		}

		if (TypeChecks.ImplementsIEnumerable(sourceType))
		{
			var innerType = TypeChecks.GetGenericType(sourceType);

			var (TypeName, FullName, _, IsBuiltin, _, IsReadonly, IsNullable, _) = GetDestinationType(innerType, customAttributes, isReadonly, isNullable);
			return new DestinationType(TypeName, FullName, IsBuiltin, true, IsReadonly, IsNullable, innerType);
		}

		if (TypeChecks.IsValueTuple(sourceType))
		{
			var arguments = sourceType.GenericTypeArguments;
			var argumentDestinationTypes = arguments.Select(arg => GetDestinationType(arg, customAttributes, isReadonly, isNullable));
			var isBuiltin = argumentDestinationTypes.All(arg => arg.IsBuiltin);

			var argumentList = argumentDestinationTypes.Select((arg, idx) => $"item{idx + 1}: {arg.FullTypeName}");
			var typeName = $"{{ {string.Join(", ", argumentList)} }}";

			return new DestinationType(typeName, sourceType.FullName, isBuiltin, false, isReadonly, false, null);
		}

		if (TypeChecks.IsNullable(sourceType))
		{
			return GetDestinationType(sourceType.GenericTypeArguments.First(), customAttributes, isReadonly, true);
		}

		if (customAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.DynamicAttribute"))
			return new DestinationType(DestinationTypes.Dynamic, null, true, false, isReadonly, true, null);

		// FIXME: Check if this is one of our types?
		var outputType = Convert(sourceType);
		// FIXME: We should probably do some conversion of the generic attribute types here
		CustomMappedTypes.Add(sourceType, outputType);
		return new DestinationType(outputType.Name, outputType.FullName, false, false, isReadonly, isNullable || TypeChecks.IsNullable(sourceType), null);

		// throw new ArgumentException($"Unexpected type: {sourceType}");
	}
}
