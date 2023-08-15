using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Custom;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Services;

public interface IActivity : IDisposable, IEnumerable<IActivity>
{
    string Name { get; }

    IDictionary<string, object?> Items { get; }

    void LogTrace(string name, string? message, object? details, object? attachment, bool isFinal = false);
}

public class ActivityContext : IActivity
{
    public ActivityContext(string name, ILogger logger)
    {
        Name = name;
        Logger = logger;
        Items = new SoftDictionary<object?>();
        Items.SetItem(Strings.Items.Owner, Logger.Owner);
        Chain = AsyncScope.Push(this);
    }

    public string Name { get; }

    private ILogger Logger { get; }

    public IDictionary<string, object?> Items { get; }

    private bool IsComplete { get; set; }

    private IDisposable Chain { get; }

    public void LogTrace(string name, string? message, object? details, object? attachment, bool isFinal = false)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException($"Cannot trace '{name}' because the '{Name}' activity is complete.");
        }

        IsComplete = isFinal;
        var context = new TraceContext { Activity = this };
        context
            .Entry
            .SetItem(Strings.Items.Activity, Name)
            .SetItem(Strings.Items.Trace, name)
            .SetItem(Strings.Items.Message, message)
            .SetItem(Strings.Items.Details, DetailsFactory.CreateDetails(details))
            .SetItem(Strings.Items.Attachment, attachment);

        Logger.Log(context);
    }

    public void Dispose()
    {
        // finalize the activity if the user hasn't already done that
        if (!IsComplete)
        {
            var details = new { auto = true };
            if (Items.Exception() is { } exception)
            {
                this.LogError(details: details, attachment: exception);
            }
            else
            {
                this.LogEnd(details: details);
            }
        }

        Items.Dispose();
        Chain.Dispose();
    }

    public IEnumerator<IActivity> GetEnumerator() => AsyncScope<ActivityContext>.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}