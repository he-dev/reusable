using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Reusable;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        //Examples.ConsoleColorizer();
        //Examples.SemanticExtensions();

        //Examples.Log();

        //await Experiments.ServicePipelineDemo.Test();
        //await Experiments.ServicePipelineDemo2.Test();
        //PrismDemo.Start();

        Examples.LogExample();
        //MergeAnonymous();

        //Examples.Tokenize();
        //await Examples.SendEmailAsync_Mailr();
        //await Examples.SendEmailAsync_Smtp();
        //await Demo.SendEmailAsync_Smtp();
        //await Demo.SendEmailAsync_Mailr();
    }

    private static void MergeAnonymous()
    {
        var obj1 = JsonSerializer.SerializeToDocument(new { foo = new { bar = "baz" }, baz = "qux" });
        var obj2 = JsonSerializer.SerializeToDocument(new { baz = "qxx", qux = "fox" });
        //var json1 = JsonSerializer.Deserialize<JsonDocument>(obj1).RootElement;
        //var json2 = JsonSerializer.Deserialize<JsonDocument>(obj2).RootElement;


        var properties = new HashSet<string>();
        var builder = new StringBuilder();
        using var memoryStream = new MemoryStream();
        using var jsonWriter = new Utf8JsonWriter(memoryStream);

        jsonWriter.WriteStartObject();

        foreach (var obj in new[] { obj1.RootElement, obj2.RootElement })
        {
            foreach (var property in obj.EnumerateObject())
            {
                if (properties.Add(property.Name))
                {
                    property.WriteTo(jsonWriter);
                }
            }
        }

        jsonWriter.WriteEndObject();
        jsonWriter.Flush();

        var mergedJson = Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}

internal static class StringBuilderExtensions
{
    public static bool Any(this StringBuilder stringBuilder) => stringBuilder.Length > 0;

    public static StringBuilder RemoveLast(this StringBuilder stringBuilder)
    {
        if (stringBuilder.Any())
        {
            stringBuilder.Length--;
        }

        return stringBuilder;
    }
}