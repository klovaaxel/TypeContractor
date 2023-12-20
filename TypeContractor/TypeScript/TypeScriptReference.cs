using TypeContractor.Output;

namespace TypeContractor.TypeScript;

internal class TypeScriptReference
{
    public OutputType Type { get; set; }
    public IEnumerable<OutputProperty> Properties { get; set; }
}
