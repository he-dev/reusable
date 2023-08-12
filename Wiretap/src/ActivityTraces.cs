using System.Runtime.CompilerServices;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class ActivityTraces
{
    public static void LogTrace(this ActivityContext activity, string? message, object? details, object? attachment, bool isFinal, [CallerMemberName] string name = "")
    {
        activity.LogTrace(name, message, details, attachment, isFinal);
    }

    internal static void LogBegin(this ActivityContext activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTrace(message, details, attachment, false);
    }

    /// <summary>
    /// Some snapshot.
    /// </summary>
    public static void LogInfo(this ActivityContext activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTrace(message, details, attachment, false);
    }

    /// <summary>
    /// There's nothing to do.
    /// </summary>
    public static void LogNoop(this ActivityContext activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTrace(message, details, attachment, true);
    }

    /// <summary>
    /// There's something missing.
    /// </summary>
    public static void LogStop(this ActivityContext activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTrace(message, details, attachment, true);
    }

    /// <summary>
    /// Everything went fine.
    /// </summary>
    public static void LogEnd(this ActivityContext activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTrace(message, details, attachment, true);
    }

    /// <summary>
    /// Something went wrong.
    /// </summary>
    public static void LogError(this ActivityContext activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTrace(message, details, attachment, true);
    }
}