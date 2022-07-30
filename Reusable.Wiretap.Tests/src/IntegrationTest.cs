using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Text.Json;
using System.Text.Json.Custom;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Channels;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Middleware;
using Telerik.JustMock.XUnit;
using Xunit;

namespace Reusable.Wiretap.Tests;

public class IntegrationTest
{
    [Fact]
    public void FeatureTests()
    {
        var itemCount = 10;
        var timestamps = new
        {
            Expected = Enumerable.Range(1, itemCount).Select(x => new DateTime(2022, 5, 1, 10, x, 0)).ToQueue(),
            Actual = Enumerable.Range(1, itemCount).Select(x => new DateTime(2022, 5, 1, 10, x, 0)).ToQueue(),
        };

        var elapsed = new
        {
            Expected = Enumerable.Range(1, itemCount).Select(x => TimeSpan.FromTicks(x)).ToQueue(),
            Actual = Enumerable.Range(1, itemCount).Select(x => TimeSpan.FromTicks(x)).ToQueue(),
        };

        var pipelineBuilder = new TelemetryLoggerBuilder
        {
            Settings =
            {
                new Attach<IRegularProperty>(LogProperty.Names.Environment(), "Demo"),
                new AttachTimestamp(new DateTimeFactory(timestamps.Actual.Dequeue))
            },
            UnitOfWorkFeatures = new List<Func<ILoggerMiddleware>>
            {
                () => new UnitOfWorkCorrelation(),
                () => new UnitOfWorkElapsed
                {
                    Stopwatch = new LambdaStopwatch(elapsed.Actual.Dequeue, () => { }),
                },
                () => new UnitOfWorkBuffer()
            },
            Serializers =
            {
                { new SerializeTimeSpan("Elapsed") { GetValue = ts => ts.Ticks }, x => x.Name }
            },
            Channels = { new MemoryChannel() },
        };

        var loggerFactory = new LoggerFactory(pipelineBuilder);
        var logger = loggerFactory.CreateLogger<IntegrationTest>();


        var memory = logger.Node<MemoryChannel>();

        using (var unitOfWorkDefault = logger.BeginUnitOfWork(id: "foo"))
        {
            // memory
            //     .Entries
            //     .Last()
            //     .TimestampIsDateTime()
            //     .CorrelationEquals(new[] { "foo" })
            //     .LayerEquals("Application")
            //     .CategoryEquals("UnitOfWork")
            //     .IdentifierEquals("FeatureTests")
            //     .SnapshotEquals(new { flow = "Started" })
            //     .ElapsedIsLong();

            Ass3rt.Equal
            (
                LogEntry
                    .Empty()
                    .Logger("IntegrationTest")
                    .Environment("Demo")
                    .Timestamp(timestamps.Expected.Dequeue())
                    .Correlation(new[] { "foo" })
                    .Layer("Application")
                    .Category("UnitOfWork")
                    .Identifier("FeatureTests")
                    .Snapshot(new { flow = "Started" })
                    .Elapsed(elapsed.Expected.Dequeue()),
                memory.Entries.Last(),
                new[] { "CallerMemberName", "CallerLineNumber", "CallerFilePath" }
            );
        }

        // memory
        //     .Entries
        //     .Last()
        //     .TimestampIsDateTime()
        //     .CorrelationEquals(new[] { "foo" })
        //     .LayerEquals("Application")
        //     .CategoryEquals("UnitOfWork")
        //     .IdentifierEquals("FeatureTests")
        //     .SnapshotEquals(new { flow = "Completed" })
        //     .ElapsedIsLong();

        logger.Log(Telemetry.Collect.Application().Metadata(new { foo = 1 }));

        // memory
        //     .Entries
        //     .Last()
        //     .TimestampIsDateTime()
        //     .IsNull("Correlation")
        //     .LayerEquals("Application")
        //     .CategoryEquals("Metadata")
        //     .IdentifierEquals("foo")
        //     .SnapshotEquals(1)
        //     .IsNull("Elapsed");
    }
}

public class Ass3rt : Assert
{
    public static void Equal(ILogEntry expected, ILogEntry actual, IEnumerable<string> ignoreProperties)
    {
        var eNames = expected.Select(x => x.Name);
        var aNames = actual.OfType<IRegularProperty>().Select(x => x.Name);

        foreach (var name in eNames.Union(aNames).Except(ignoreProperties))
        {
            var eProperty = expected[name];
            var aProperty = actual[name];

            //IsType<ILogProperty<IRegularProperty>>(eProperty);
            //IsType<ILogProperty<IRegularProperty>>(aProperty);

            switch (aProperty)
            {
                case { Name: "Correlation" }:
                case { Name: "Snapshot" }:
                    Equal(eProperty.Value, JsonSerializer.Deserialize((string)aProperty.Value, eProperty.Value.GetType()));
                    break;
                case { Name: "Elapsed" }:
                    Equal(eProperty.Value, TimeSpan.FromTicks((long)aProperty.Value));
                    break;
                default:
                    Equal(eProperty.Value, aProperty.Value);
                    break;
            }
        }
    }

    private new static void IsType<T>(object actual)
    {
        if (actual is not T)
        {
            throw new AssertFailedException($"Expected type {typeof(T).ToPrettyString()}, but found {actual.GetType().ToPrettyString()}.");
        }
    }
}

public static class TestHelpers
{
    public static ILogEntry TimestampIsDateTime(this ILogEntry entry)
    {
        Assert.IsType<DateTime>(entry["Timestamp"].Value);
        return entry;
    }

    public static ILogEntry CorrelationEquals<T>(this ILogEntry entry, T correlation)
    {
        Assert.Equal(correlation, entry[nameof(correlation)].Deserialize(correlation));
        return entry;
    }

    public static ILogEntry LayerEquals(this ILogEntry entry, string layer)
    {
        Assert.Equal(layer, entry[nameof(layer)].Value);
        return entry;
    }

    public static ILogEntry CategoryEquals(this ILogEntry entry, string category)
    {
        Assert.Equal(category, entry[nameof(category)].Value);
        return entry;
    }

    public static ILogEntry IdentifierEquals(this ILogEntry entry, string identifier)
    {
        Assert.Equal(identifier, entry[nameof(identifier)].Value);
        return entry;
    }

    public static ILogEntry SnapshotEquals<TSnapshot>(this ILogEntry entry, TSnapshot snapshot)
    {
        Assert.Equal(snapshot, entry[nameof(snapshot)].Deserialize(snapshot));
        return entry;
    }

    public static ILogEntry ElapsedIsLong(this ILogEntry entry)
    {
        Assert.IsType<long>(entry["Elapsed"].Value);
        return entry;
    }

    public static ILogEntry IsNull(this ILogEntry entry, string name)
    {
        Assert.IsType<LogProperty.Null>(entry[name]);
        return entry;
    }

    private static T? Deserialize<T>(this ILogProperty property, T typeObject) => JsonSerializer.Deserialize<T>(property.Value<string>());
}