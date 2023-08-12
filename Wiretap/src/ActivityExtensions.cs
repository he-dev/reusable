using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class ActivityExtensions
{
    public static IDisposable PushItem(this ActivityContext activity, string name, object value)
    {
        activity.Items.Set(name, value);
        return Disposable.From(() => activity.Items.Remove(name));
    }

    public static IDisposable PushCallerInfo
    (
        this ActivityContext activity,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return activity.PushItem("CallerInfo", new
        {
            memberName = callerMemberName,
            lineNumber = callerLineNumber,
            filePath = callerFilePath
        });
    }

    public static IDisposable PushDetail(this ActivityContext activity, object value)
    {
        return activity.PushItem(LogEntry.PropertyNames.Details, DetailsFactory.CreateDetails(value));
    }
}