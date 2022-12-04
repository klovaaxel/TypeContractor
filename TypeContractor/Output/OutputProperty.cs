namespace TypeContractor.Output;

public record OutputProperty(string SourceName, Type SourceType, Type? InnerSourceType, string DestinationName, string DestinationType, bool IsBuiltin, bool IsArray, bool IsNullable)
{
    public override string ToString()
    {
        return $"{DestinationName}{(IsNullable ? "?" : "")}: {DestinationType}{(IsArray ? "[]" : "")} (from {SourceType}, {(IsBuiltin ? "builtin" : "custom")})";
    }
}