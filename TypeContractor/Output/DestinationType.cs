namespace TypeContractor.Output;

public record DestinationType(string TypeName, bool IsBuiltin, bool IsArray, Type? InnerType);