using System.Globalization;

namespace TypeContractor;

public class TypeContractorConfiguration
{
    private const string DstString = "string";
    private const string DstBool = "boolean";
    private const string DstNumber = "number";
    private const string DstByteArray = "number[]";

    private readonly Dictionary<Type, string> _map = new();
    private readonly List<string> _suffixes = new();
    private readonly Dictionary<string, string> _assemblies = new();
    private readonly Dictionary<string, string> _replacements = new();
    private string? _outputPath;

    public IReadOnlyDictionary<Type, string> TypeMaps => _map;
    public IReadOnlyList<string> Suffixes => _suffixes;
    public IReadOnlyDictionary<string, string> Assemblies => _assemblies;
    public IReadOnlyDictionary<string, string> Replacements => _replacements;
    public string OutputPath => _outputPath ?? throw new InvalidOperationException("Output path is not configured");

    public static TypeContractorConfiguration WithDefaultConfiguration()
    {
        return new TypeContractorConfiguration()
            .AddDefaultSuffixes()
            .AddDefaultTypeMaps();
    }

    public TypeContractorConfiguration AddDefaultSuffixes()
    {
        _suffixes.AddRange(new[] { "Dto", "Request", "Response" });
        return this;
    }

    public TypeContractorConfiguration AddDefaultTypeMaps()
    {
        _map.Add(typeof(string), DstString);
        _map.Add(typeof(DateTime), DstString);
        _map.Add(typeof(DateTimeOffset), DstString);
        _map.Add(typeof(Guid), DstString);
        _map.Add(typeof(bool), DstBool);
        _map.Add(typeof(byte), DstNumber);
        _map.Add(typeof(short), DstNumber);
        _map.Add(typeof(int), DstNumber);
        _map.Add(typeof(long), DstNumber);
        _map.Add(typeof(decimal), DstNumber);
        _map.Add(typeof(float), DstNumber);
        _map.Add(typeof(Stream), DstByteArray);
        _map.Add(typeof(CultureInfo), DstString);

        return this;
    }

    public TypeContractorConfiguration AddCustomMap(Type type, string destinationType)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (!_map.TryAdd(type, destinationType))
            throw new InvalidOperationException($"Unable to add {type} to list of maps. Already added?");

        return this;
    }

    public TypeContractorConfiguration AddSuffix(params string[] suffixes)
    {
        _suffixes.AddRange(suffixes);
        return this;
    }

    public TypeContractorConfiguration AddAssembly(string assemblyName, string assemblyPath)
    {
        if (!_assemblies.TryAdd(assemblyName, assemblyPath))
            throw new InvalidOperationException($"Unable to add {assemblyName}. Already added?");

        return this;
    }

    public TypeContractorConfiguration AddReplacement(string searchString, string replacement)
    {
        if (!_replacements.TryAdd(searchString, replacement))
            throw new InvalidOperationException($"Unable to add replacement {searchString}. Already added?");

        return this;
    }

    public TypeContractorConfiguration SetOutputDirectory(string outputDirectory)
    {
        _outputPath = outputDirectory;

        return this;
    }
}
