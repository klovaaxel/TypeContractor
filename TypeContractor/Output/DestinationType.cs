namespace TypeContractor.Output;

public record DestinationType(string TypeName, string ImportType, bool IsBuiltin, bool IsArray, Type? InnerType)
{
    public DestinationType(string typeName, bool isBuiltin, bool isArray, Type? innerType, string? importType = null) : this(typeName, importType ?? typeName, isBuiltin, isArray, innerType)
    {
    }

    /// <summary>
    /// Returns the <see cref="TypeName"/> and array brackets if the type is an array
    /// </summary>
    public string FullTypeName => $"{TypeName}{(IsArray ? "[]" : "")}";
}
