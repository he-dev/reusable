using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Custom;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services;

public interface IActivity : IEnumerable<IActivity>
{
    string Name { get; }

    IMemoryCache Items { get; }

    void LogTrace(string name, string? message, object? details, object? attachment, bool isFinal = false);
}

public class ActivityContext : IDisposable, IActivity
{
    public ActivityContext()
    {
        Chain = AsyncScope.Push(this);
    }

    public required string Name { get; init; }

    public required LogFunc Log { get; init; }

    public IMemoryCache Items { get; } = new MemoryCache(new MemoryCacheOptions());

    private bool IsComplete { get; set; }

    private IDisposable Chain { get; }

    public void LogTrace(string name, string? message, object? details, object? attachment, bool isFinal = false)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException($"Cannot trace '{name}' because this '{Name}' activity is complete.");
        }

        IsComplete = isFinal;

        details = DetailsFactory.CreateDetails(details);

        Log(
            this,
            LogEntry
                .Empty()
                .SetItem(LogEntry.PropertyNames.Trace, name)
                .SetItem(LogEntry.PropertyNames.Message, message)
                .SetItem(LogEntry.PropertyNames.Details, details)
                .SetItem(LogEntry.PropertyNames.Attachment, attachment)
        );
    }

    public void Dispose()
    {
        // finalize the activity if the user hasn't already done that
        if (!IsComplete)
        {
            if (Items.Exception() is { } exception)
            {
                this.LogError(attachment: exception);
            }
            else
            {
                this.LogEnd();
            }
        }

        Items.Dispose();
        Chain.Dispose();
    }

    public IEnumerator<IActivity> GetEnumerator() => AsyncScope<ActivityContext>.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

