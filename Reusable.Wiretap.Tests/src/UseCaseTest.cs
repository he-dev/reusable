using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.ChannelFilters;
using Reusable.Wiretap.Channels;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Xunit;

namespace Reusable.Wiretap.Tests;

public class UseCaseTest
{
    public UseCaseTest()
    {
        var pipelineBuilder = new LoggerBuilder
        {
            Settings =
            {
                new Attach<IRegularProperty>(LogProperty.Names.Environment(), "Demo"),
                //new Lambda(e => e.OptIn<MemoryChannel>())
            },
            Channels = { new MemoryChannel() },
            ChannelFilter = new ChannelOpt()
        };

        LoggerFactory = new LoggerFactory(pipelineBuilder);
        Logger = LoggerFactory.CreateLogger<UseCaseTest>();
    }

    private ILoggerFactory LoggerFactory { get; }

    private ILogger Logger { get; }

    [Fact]
    public void Can_log_Message()
    {
        //var entry = LogEntry.Empty();
        Logger.Log(e => e.Message("Test!"));

        var memory = Logger.Node<MemoryChannel>();
        var entry = memory.Entries.Single();
        var regularProperties = entry.WhereTag<IRegularProperty>();
        Assert.Equal(11, regularProperties.Count());
        Assert.Equal("Test!", entry["Message"].Value);
    }

    [Fact]
    public void Can_log_scope()
    {
        using var scope = Logger.BeginScope(nameof(Can_log_scope));

        //var entry = LogEntry.Empty();
        Logger.Log(e => e.Message("Test!"));

        var memory = Logger.Node<MemoryChannel>();
        var entry = memory.Entries.Single();
        var regularProperties = entry.WhereTag<IRegularProperty>();
        Assert.Equal(3, regularProperties.Count());
        Assert.Equal("Test!", entry["Message"].Value);
    }
}