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
    // FIXME: Arguments or configuration
    public static readonly Dictionary<string, string> Replacements = new()
    {
        { "Hogia.SalaryService.Common.", string.Empty }
    };

    private static void Main(string[] args)
    {
        // FIXME: Read as arguments
        var assemblies = new Dictionary<string, string>
        {
            { "Hogia.SalaryService.Common", "../../../../deps/Hogia.SalaryService.Common.dll" }
        };

        // FIXME: Arguments, or make configurable
        var suffixes = new[]
        {
            "Dto",
            "Request",
            "Response"
        };

        // FIXME: Should be configurable
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output");
        var toConvert = new List<ContractedType>();

        // FIXME: Cleanup while debugging
#if DEBUG
        Benchmark.Measure("Cleanup", () =>
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
                Directory.CreateDirectory(outputPath);
            }
        });
#endif

        foreach (var (assemblyName, assemblyPath) in assemblies)
        {
            var sw = Benchmark.Start(assemblyName, writeInitial: true);
            var assembly = Assembly.LoadFrom(assemblyPath);

            var types = assembly.GetTypes()
                .Where(t => suffixes.Any(suffix => t.Name.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)))
                .Select(t => ContractedType.FromName(t.FullName!, t))
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

            var converter = new TypeScriptConverter();
            var writer = new TypeScriptWriter(outputPath);

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