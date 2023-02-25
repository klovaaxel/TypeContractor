using System.Runtime.CompilerServices;
using TypeContractor.Helpers;

[assembly: InternalsVisibleTo("TypeContractor.Tests")]
[assembly: CLSCompliant(true)]
namespace TypeContractor;

public static class Program
{
    private static void Main(string[] args)
    {
        var contractor = Contractor.FromDefaultConfiguration(configuration => configuration
            .AddAssembly("ExampleContracts", "ExampleContracts.dll")
            .SetOutputDirectory(Path.Combine(Directory.GetCurrentDirectory(), "output")));

        // FIXME: Cleanup while debugging
#if DEBUG
        Benchmark.Measure("Cleanup", () =>
        {
            var configuration = contractor.Configuration;
            if (Directory.Exists(configuration.OutputPath))
            {
                Directory.Delete(configuration.OutputPath, true);
                Directory.CreateDirectory(configuration.OutputPath);
            }
        });
#endif

        contractor.Build();
    }
}