using System.Globalization;
using TypeContractor.TypeScript;

namespace TypeContractor;

public class TypeContractorConfiguration
{
    private readonly Dictionary<string, string> _map = new();
    private readonly List<string> _suffixes = new();
    private readonly Dictionary<string, string> _assemblies = new();
    private readonly Dictionary<string, string> _replacements = new();
    private string? _outputPath;

    public IReadOnlyDictionary<string, string> TypeMaps => _map;
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
        AddCustomMap(typeof(string), DestinationTypes.StringType);
        AddCustomMap(typeof(DateTime), DestinationTypes.StringType);
        AddCustomMap(typeof(DateTimeOffset), DestinationTypes.StringType);
        AddCustomMap(typeof(Guid), DestinationTypes.StringType);
        AddCustomMap(typeof(bool), DestinationTypes.Boolean);
        AddCustomMap(typeof(byte), DestinationTypes.Number);
        AddCustomMap(typeof(short), DestinationTypes.Number);
        AddCustomMap(typeof(int), DestinationTypes.Number);
        AddCustomMap(typeof(long), DestinationTypes.Number);
        AddCustomMap(typeof(decimal), DestinationTypes.Number);
        AddCustomMap(typeof(float), DestinationTypes.Number);
        AddCustomMap(typeof(Stream), DestinationTypes.ByteArray);
        AddCustomMap(typeof(CultureInfo), DestinationTypes.StringType);

        return this;
    }

    /// <summary>
    /// Add a custom mapping from a type.
    /// 
    /// <para>
    /// A list of default destination types can be found in <see cref="DestinationTypes"/>
    /// </para>
    /// 
    /// <para>
    /// If the type isn't available at compile time, use <see cref="AddCustomMap(string, string)"/> instead.
    /// </para>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="destinationType"></param>
    /// <returns>The configuration object for continued chaining</returns>
    public TypeContractorConfiguration AddCustomMap(Type type, string destinationType)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return AddCustomMap(type.FullName!, destinationType);
    }

    /// <summary>
    /// Add a custom mapping from a type, specified as a string.
    /// 
    /// <para>
    /// A list of default destination types can be found in <see cref="DestinationTypes"/>
    /// </para>
    /// 
    /// <para>
    /// If the type is available at compile time, use <see cref="AddCustomMap(Type, string)"/> instead for improved
    /// type safety and resilience against refactoring and renames.
    /// </para>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="destinationType"></param>
    /// <returns>The configuration object for continued chaining</returns>
    public TypeContractorConfiguration AddCustomMap(string type, string destinationType)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type));

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
