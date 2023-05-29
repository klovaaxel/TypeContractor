using System.Reflection;
using System.Runtime.InteropServices;
using TypeContractor.Output;
using TypeContractor.TypeScript;

namespace TypeContractor;

public class Contractor
{
    private readonly TypeContractorConfiguration _configuration;

    public static Contractor WithConfiguration(TypeContractorConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        return new Contractor(configuration);
    }

    /// <summary>
    /// Construct a <see cref="Contractor"/> using <see cref="TypeContractorConfiguration.WithDefaultConfiguration"/>
    /// and provide an action to continue configuring Contractor.
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configurationBuilder"/> is null</exception>
    public static Contractor FromDefaultConfiguration(Action<TypeContractorConfiguration> configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder, nameof(configurationBuilder));

        var configuration = TypeContractorConfiguration.WithDefaultConfiguration();
        configurationBuilder(configuration);

        return new Contractor(configuration);
    }

    private Contractor(TypeContractorConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TypeContractorConfiguration Configuration => _configuration;

    public void Build(MetadataLoadContext? metadataLoadContext = null)
    {
        metadataLoadContext ??= BuildMetadataLoadContext();
        var toConvert = new List<ContractedType>();

        foreach (var (assemblyName, assemblyPath) in _configuration.Assemblies)
        {
            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyPath);

            var types = assembly.GetTypes()
                .Where(t => _configuration.Types.Any(type => type == t.FullName) || _configuration.Suffixes.Any(suffix => t.Name.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)))
                .Select(t => ContractedType.FromName(t.FullName!, t, _configuration))
                .ToList();

            var folders = types
                .Select(t => t.Folder)
                .DistinctBy(x => x.Path)
                .OrderBy(x => x.Path)
                .ToList();

            foreach (var folder in folders)
            {
                var items = types
                    .Where(n => n.Folder == folder);

                toConvert.AddRange(items);
            }

            var converter = new TypeScriptConverter(_configuration, metadataLoadContext);
            var writer = new TypeScriptWriter(_configuration.OutputPath);

            var outputTypes = types
                .Select(converter.Convert)
                .ToList() // Needed so `converter.Convert` runs before we concat
                .Concat(converter.CustomMappedTypes.Values)
                .ToList();

            foreach (var type in outputTypes)
            {
                writer.Write(type, outputTypes);
            }
        }
    }

    private MetadataLoadContext BuildMetadataLoadContext()
    {
        // Get the array of runtime assemblies.
        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

        // Create the list of assembly paths consisting of runtime assemblies and the inspected assemblies.
        var paths = runtimeAssemblies.Concat(_configuration.Assemblies.Values);

        var resolver = new PathAssemblyResolver(paths);

        return new MetadataLoadContext(resolver);
    }
}
