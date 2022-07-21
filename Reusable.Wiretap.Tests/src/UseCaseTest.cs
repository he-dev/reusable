using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Channels;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Xunit;

namespace Reusable.Wiretap.Tests;

public class UseCaseTest : IDisposable
{
    public UseCaseTest()
    {
        //MemoryChannel = new MemoryChannel();

        var pipelineBuilder = new LoggerPipelineBuilder
        {
            Properties = new[]
            {
                new Attach<IRegularProperty>(LogProperty.Names.Environment(), "Demo")
            },
            Channels = new[] { new MemoryChannel() }
        };

        LoggerFactory = new LoggerFactory(pipelineBuilder);
        Logger = LoggerFactory.CreateLogger<UseCaseTest>();
    }

    //private MemoryChannel MemoryChannel { get; }

    private ILoggerFactory LoggerFactory { get; }

    private ILogger Logger { get; }

    [Fact]
    public void Logs_Regular_Properties()
    {
        //var entry = LogEntry.Empty();
        Logger.Log(e => e.Message("Test!"));

        var memory = Logger.Node<MemoryChannel>();
        var entry = memory.Single();
        var regularProperties = entry.WhereTag<IRegularProperty>();
        Assert.Equal(3, regularProperties.Count());
        Assert.Equal("Test!", memory.Single()["Message"].Value);
    }

    public void Dispose()
    {
        Logger.Dispose();
        LoggerFactory.Dispose();
    }
}