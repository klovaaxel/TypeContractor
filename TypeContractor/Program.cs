using System.Reflection;
using System.Runtime.CompilerServices;
using TypeContractor.Helpers;
using TypeContractor.Output;
using TypeContractor.TypeScript;

[assembly: InternalsVisibleTo("TypeContractor.Tests")]
[assembly: CLSCompliant(true)]
namespace TypeContractor;

public static class Program
{
    private static void Main(string[] args)
    {
        var configuration = Configuration
            .WithDefaultConfiguration()
            .AddAssembly("ExampleContracts", "ExampleContracts.dll")
            .SetOutputDirectory(Path.Combine(Directory.GetCurrentDirectory(), "output"));
        var toConvert = new List<ContractedType>();

        // FIXME: Cleanup while debugging
#if DEBUG
        Benchmark.Measure("Cleanup", () =>
        {
            if (Directory.Exists(configuration.OutputPath))
            {
                Directory.Delete(configuration.OutputPath, true);
                Directory.CreateDirectory(configuration.OutputPath);
            }
        });
#endif

        foreach (var (assemblyName, assemblyPath) in configuration.Assemblies)
        {
            var sw = Benchmark.Start(assemblyName, writeInitial: true);
            var assembly = Assembly.LoadFrom(assemblyPath);

            var types = assembly.GetTypes()
                .Where(t => configuration.Suffixes.Any(suffix => t.Name.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)))
                .Select(t => ContractedType.FromName(t.FullName!, t, configuration))
                .ToList();

            Console.WriteLine($"    Found {types.Count} types that matches the suffix");

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

            var converter = new TypeScriptConverter(configuration);
            var writer = new TypeScriptWriter(configuration.OutputPath);

            var cbm = Benchmark.Start("Conversion");
            var outputTypes = types
                .Select(converter.Convert)
                .ToList();
            cbm.Stop();

            outputTypes = outputTypes.Concat(converter.CustomMappedTypes.Values).ToList();

            var wbm = Benchmark.Start("Writing", writeInitial: true);
            foreach (var type in outputTypes)
            {
                writer.Write(type, outputTypes);
            }
            wbm.Stop();
            sw.Stop();
        }
    }
}