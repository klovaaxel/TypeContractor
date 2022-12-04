namespace TypeContractor.Output;

public record OutputEnumMember(string SourceName, object SourceValue, string DestinationName, object DestinationValue)
{
    public override string ToString()
    {
        return $"{DestinationName} = {DestinationValue} (from {SourceValue})";
    }
}