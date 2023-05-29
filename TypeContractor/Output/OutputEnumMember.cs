namespace TypeContractor.Output;

public record OutputEnumMember(string SourceName, string DestinationName, object DestinationValue)
{
    public override string ToString()
    {
        return $"{DestinationName} = {DestinationValue}";
    }
}