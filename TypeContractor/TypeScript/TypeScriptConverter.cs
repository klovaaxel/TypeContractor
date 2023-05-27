using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TypeContractor.Helpers;
using TypeContractor.Output;

namespace TypeContractor.TypeScript;

public class TypeScriptConverter
{
    private readonly TypeContractorConfiguration _configuration;

    public TypeScriptConverter(TypeContractorConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Dictionary<Type, OutputType> CustomMappedTypes { get; } = new();

    public OutputType Convert(ContractedType contractedType)
    {
        if (contractedType is null) throw new ArgumentNullException(nameof(contractedType));
        return Convert(contractedType.Type, contractedType);
    }

    public OutputType Convert(Type type, ContractedType? contractedType = null)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        return new(
            type.Name,
            type.FullName!,
            contractedType ?? ContractedType.FromName(type.FullName!, type, _configuration),
            type.IsEnum,
            type.IsEnum ? null : GetProperties(type).Distinct().ToList(),
            type.IsEnum ? GetEnumProperties(type) : null
        );
    }

    private static ICollection<OutputEnumMember> GetEnumProperties(Type type) =>
        type
            .GetEnumNames()
            .Select(name =>
            {
                var value = Enum.Parse(type, name);
                var numericValue = type.GetField("value__")!.GetValue(value);

                return new OutputEnumMember(name, value, name, numericValue!);
            })
            .ToList();

    private ICollection<OutputProperty> GetProperties(Type type)
    {
        var outputProperties = new List<OutputProperty>();

        // Find all properties
        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Evaluate type of property
        foreach (var property in properties)
        {
            // Need to have a getter
            if (!property.CanRead) continue;

            // Getter has to be public
            var getter = property.GetGetMethod(false);
            if (getter is null) continue;

            var destinationName = GetDestinationName(property.Name);
            var destinationType = GetDestinationType(property.PropertyType);
            outputProperties.Add(new OutputProperty(property.Name, property.PropertyType, destinationType.InnerType, destinationName, destinationType.TypeName, destinationType.IsBuiltin, destinationType.IsArray, TypeChecks.IsNullable(property.PropertyType)));
        }

        // Look at base classes
        if (type.BaseType is not null)
            outputProperties.AddRange(GetProperties(type.BaseType));

        // Look at interfaces
        foreach (var iface in type.GetInterfaces())
            outputProperties.AddRange(GetProperties(iface));

        return outputProperties;
    }

    private static string GetDestinationName(string name) => name.ToTypeScriptName();

    private DestinationType GetDestinationType(in Type sourceType)
    {
        if (_configuration.TypeMaps.TryGetValue(sourceType.FullName!, out string? destType))
            return new DestinationType(destType, true, false, null);

        if (CustomMappedTypes.TryGetValue(sourceType, out OutputType? customType))
            return new DestinationType(customType.Name, false, false, null);

        if (TypeChecks.ImplementsIEnumerable(sourceType))
        {
            var innerType = sourceType.GenericTypeArguments.First();
            var (TypeName, IsBuiltin, _, _) = GetDestinationType(innerType);
            return new DestinationType(TypeName, IsBuiltin, true, innerType);
        }

        if (TypeChecks.IsNullable(sourceType))
        {
            return GetDestinationType(sourceType.GenericTypeArguments.First());
        }

        // FIXME: Check if this is one of our types?
        var outputType = Convert(sourceType);
        CustomMappedTypes.Add(sourceType, outputType);
        return new DestinationType(outputType.Name, false, false, null);

        // throw new ArgumentException($"Unexpected type: {sourceType}");
    }
}
