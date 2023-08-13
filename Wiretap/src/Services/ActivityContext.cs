using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Services;

public interface IActivity : IEnumerable<IActivity>
{
    string Name { get; }

    IDictionary<string, object?> Items { get; }

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

    public IDictionary<string, object?> Items { get; } = new SoftDictionary();

    private bool IsComplete { get; set; }

    private IDisposable Chain { get; }

    public void LogTrace(string name, string? message, object? details, object? attachment, bool isFinal = false)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException($"Cannot trace '{name}' because this '{Name}' activity is complete.");
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

        Log(context);
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