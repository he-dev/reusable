using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class ActivityExtensions
{
    public static IDisposable PushItem(this IDictionary<string, object?> source, string name, object value)
    {
        source.SetItem(name, value);
        return Disposable.From(() => source.RemoveItem(name));
    }

    public static IDisposable PushCallerInfo
    (
        this IDictionary<string, object?> source,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return source.PushItem(Strings.Items.Caller, new
        {
            memberName = callerMemberName,
            lineNumber = callerLineNumber,
            filePath = callerFilePath
        });
    }

    public static IDisposable PushDetails(this IDictionary<string, object?> source, object value)
    {
        return source.PushItem(Strings.Items.Details, DetailsFactory.CreateDetails(value));
    }
}