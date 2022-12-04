namespace TypeContractor.Output;

public record ContractedType(string Name, string FullName, Type Type, Folder Folder)
{
    public static ContractedType FromName(string name, Type type)
    {
        var nameparts = ApplyReplacements(name).Split('.');
        var typeName = nameparts.Last();
        var folderName = nameparts.Take(..^1);
        return new ContractedType(typeName, name, type, Folder.FromParts(folderName.ToArray()));
    }

    public static string ApplyReplacements(string input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));

        foreach (var (needle, replacement) in Program.Replacements)
            input = input.Replace(needle, replacement, StringComparison.InvariantCulture);

        return input;
    }
}