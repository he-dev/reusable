using System;
using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;

namespace Reusable.Octopus.Extensions;

public static class RequestExtensions
{
    public static string CurrentName(this Request request) => request.ResourceName.Peek();
    
    public static object CurrentBody(this Request request) => request.Body.Peek();
    
    public static bool IsControlledBy(this Request request, IResourceController controller)
    {
        return controller.RequestType.IsInstanceOfType(request) && (request.ControllerFilter is null || request.ControllerFilter.Matches(controller));
    }

    public static T GetItemOrDefault<T>(this Request request, string name, T defaultValue)
    {
        return
            request.Items.TryGetValue(name, out var value) && value is T result
                ? result
                : defaultValue;
    }

    public static T GetItem<T>(this Request request, string name)
    {
        return
            request.Items.TryGetValue(name, out var value) && value is T result
                ? result
                : throw DynamicException.Create("ItemNotFound", $"Could not find item '{name}'.");
    }

    public static void Required(this Request request, bool required)
    {
        request.Items[nameof(Required)] = required;
    }

    public static bool Required(this Request request)
    {
        return request.Items.TryGetValue(nameof(Required), out var value) && value is true;
    }

    public static void CacheLifetime(this Request request, TimeSpan lifetime)
    {
        request.Items[nameof(CacheLifetime)] = lifetime;
    }

    public static TimeSpan CacheLifetime(this Request request)
    {
        return request.Items.TryGetValue(nameof(CacheLifetime), out var value) && value is TimeSpan lifetime ? lifetime : TimeSpan.Zero;
    }

    public static T Log<T>(this T source, string message) where T : IItems
    {
        return source.Also(r =>
        {
            if (r.Items.TryGetValue(nameof(Log), out var item) && item is IList<string> logs)
            {
                logs.Add(CreateLogEntry<T>(message));
            }
            else
            {
                r.Items[nameof(Log)] = new List<string> { CreateLogEntry<T>(message) };
            }
        });
    }

    private static string CreateLogEntry<T>(string message) => $"{DateTime.UtcNow} | {typeof(T).ToPrettyString()} | {message}";
}