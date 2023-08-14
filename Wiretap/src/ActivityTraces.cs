using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class ActivityTraces
{
    public static void LogTraceByCaller(this IActivity activity, string? message, object? details, object? attachment, bool isFinal, [CallerMemberName] string name = "")
    {
        activity.LogTrace(name.RegexReplace("^Log"), message, details, attachment, isFinal);
    }

    internal static void LogBegin(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, false);
    }

    public static void LogArgs(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, false);
    }

    public static void LogInfo(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, false);
    }

    public static void LogCaller
    (
        this IActivity activity,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        activity.LogTraceByCaller(default, new
        {
            caller = new
            {
                memberName = callerMemberName,
                lineNumber = callerLineNumber,
                filePath = callerFilePath
            }
        }, default, false);
    }

    public static void LogMetric(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, false);
    }

    public static void LogRequest(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, false);
    }

    public static void LogResponse(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, false);
    }

    public static void LogResult(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, true);
    }

    /// <summary>
    /// There's nothing to do.
    /// </summary>
    public static void LogNoop(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, true);
    }

    /// <summary>
    /// There's something missing.
    /// </summary>
    public static void LogBreak(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, true);
    }

    /// <summary>
    /// Everything went fine.
    /// </summary>
    public static void LogEnd(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, true);
    }

    /// <summary>
    /// Something went wrong.
    /// </summary>
    public static void LogError(this IActivity activity, string? message = default, object? details = default, object? attachment = default)
    {
        activity.LogTraceByCaller(message, details, attachment, true);
    }
}