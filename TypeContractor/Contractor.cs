﻿using System.Reflection;
using TypeContractor.Output;
using TypeContractor.TypeScript;

namespace TypeContractor;

public class Contractor
{
    private readonly TypeContractorConfiguration configuration;

    public static Contractor WithConfiguration(TypeContractorConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        return new Contractor(configuration);
    }

    public static Contractor FromDefaultConfiguration(Action<TypeContractorConfiguration> configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder, nameof(configurationBuilder));

        var configuration = TypeContractorConfiguration.WithDefaultConfiguration();
        configurationBuilder(configuration);

        return new Contractor(configuration);
    }

    private Contractor(TypeContractorConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public TypeContractorConfiguration Configuration => configuration;

    public void Build()
    {
        var toConvert = new List<ContractedType>();

        foreach (var (assemblyName, assemblyPath) in configuration.Assemblies)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);

            var types = assembly.GetTypes()
                .Where(t => configuration.Suffixes.Any(suffix => t.Name.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)))
                .Select(t => ContractedType.FromName(t.FullName!, t, configuration))
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

            var converter = new TypeScriptConverter(configuration);
            var writer = new TypeScriptWriter(configuration.OutputPath);

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
}