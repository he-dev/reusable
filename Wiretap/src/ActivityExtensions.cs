using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class ActivityExtensions
{
    public static IDisposable PushItem(this IDictionary<string, object?> source, string name, object? value)
    {
        source.SetItem(name, value);
        return Disposable.From(() => source.RemoveItem(name));
    }

    public static IDisposable PushDetails(this IDictionary<string, object?> source, object value)
    {
        return source.PushItem(Strings.Items.Details, DetailsFactory.CreateDetails(value));
    }
}