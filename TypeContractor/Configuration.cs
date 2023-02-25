using System.Globalization;

namespace TypeContractor;

public class Configuration
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

    public static Configuration WithDefaultConfiguration()
    {
        return new Configuration()
            .AddDefaultSuffixes()
            .AddDefaultTypeMaps();
    }

    public Configuration AddDefaultSuffixes()
    {
        _suffixes.AddRange(new[] { "Dto", "Request", "Response" });
        return this;
    }

    public Configuration AddDefaultTypeMaps()
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

    public Configuration AddCustomMap(Type type, string destinationType)
    {
        if (!_map.TryAdd(type, destinationType))
            throw new InvalidOperationException($"Unable to add {type} to list of maps. Already added?");

        return this;
    }

    public Configuration AddSuffix(params string[] suffixes)
    {
        _suffixes.AddRange(suffixes);
        return this;
    }

    public Configuration AddAssembly(string assemblyName, string assemblyPath)
    {
        if (!_assemblies.TryAdd(assemblyName, assemblyPath))
            throw new InvalidOperationException($"Unable to add {assemblyName}. Already added?");

        return this;
    }

    public Configuration AddReplacement(string searchString, string replacement)
    {
        if (!_replacements.TryAdd(searchString, replacement))
            throw new InvalidOperationException($"Unable to add replacement {searchString}. Already added?");

        return this;
    }

    public Configuration SetOutputDirectory(string outputDirectory)
    {
        _outputPath = outputDirectory;

        return this;
    }
}
