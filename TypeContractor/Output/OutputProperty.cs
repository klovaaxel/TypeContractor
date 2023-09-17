namespace TypeContractor.Output;

public record OutputProperty(string SourceName, Type SourceType, Type? InnerSourceType, string DestinationName, string DestinationType, string ImportType, bool IsBuiltin, bool IsArray, bool IsNullable, bool IsReadonly)
{
    public override string ToString()
    {
        return $"{(IsReadonly ? "readonly" : "")}{DestinationName}{(IsNullable ? "?" : "")}: {FullDestinationType} (import {ImportType} from {SourceType}, {(IsBuiltin ? "builtin" : "custom")})";
    }

    /// <summary>
    /// Returns the <see cref="DestinationType"/> and array brackets if the type is an array
    /// </summary>
    public string FullDestinationType => $"{DestinationType}{(IsArray ? "[]" : "")}";
}
