using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Synergy;
using Reusable.Synergy.Controllers;
using Reusable.Synergy.Requests;
using Reusable.Synergy.Services;

namespace Reusable.Experiments;

public class ServicePipelineDemo2
{
    public static async Task Test()
    {
        // Compose the container.
        var builder = new ContainerBuilder();

        // Register cache dependency,
        builder.RegisterInstance(new MemoryCache(new MemoryCacheOptions())).As<IMemoryCache>().SingleInstance();

        // Register service-pipeline-builder and associate it with ReadFile.Text by name.
        builder.Register(c => new Service.PipelineBuilder
        {
            // This pipeline should resolve environment variables that might be used by the .Name property.
            new EnvironmentVariableService(PropertyService.For<IReadFile>.Select(x => x.Name)),
            // All text files should be cached for 15min.
            new CacheLifetimeService(TimeSpan.FromMinutes(15)),
            // Use this branch for .txt-files.
            new BranchService
            {
                When = Condition.For<IReadFile>.When(x => x.Name.EndsWith(".txt"), ".txt"),
                Services = { new CacheLifetimeService(TimeSpan.FromMinutes(30)) }
            },
            new CacheService(c.Resolve<IMemoryCache>(), PropertyService.For<IReadFile>.Select(x => x.Name)),
            // This pipeline should look in embedded resources first before it reads files. 
            // Since this node doesn't have to succeed, the next one is called.
            new EmbeddedResourceService<ServicePipelineDemo2> { MustSucceed = false },
            // This overrides the file-service for testing.
            //new ConstantService.Text("This is not a real file!"),
            // Finally this node tries to read a file.
            new FileService.Read()
        }).InstancePerDependency().Named<Service.PipelineBuilder>("notes");

        await using var container = builder.Build();
        await using var scope = container.BeginLifetimeScope();

        // Set some environment variable.
        Environment.SetEnvironmentVariable("HOME", @"c:\temp");

        // Create the request and invoke it.
        var result = await new ReadFile<string>(@"%HOME%\notes.txt").Tag("notes").CacheLifetime(TimeSpan.FromMinutes(10)).InvokeAsync(scope);

        Console.WriteLine(result); // --> "This is not a real file!" or FileNotFoundException (as there is no "notes.txt".
    }
}