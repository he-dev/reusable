using System;

namespace Reusable.Wiretap;

public static class TaskStatus
{
    internal static void Started(this LoggerContext context, object? details = default, object? attachment = default) => context.Status(nameof(Started), details, attachment);

    public static void Running(this LoggerContext context, object? details = default, object? attachment = default) => context.Status(nameof(Running), details, attachment);

    public static void Completed(this LoggerContext context, object? details = default, object? attachment = default) => context.Status(nameof(Completed), details, attachment);

    public static void Canceled(this LoggerContext context, object? details = default, object? attachment = default) => context.Status(nameof(Canceled), details, attachment);

    public static void Faulted(this LoggerContext context, object? details = default, object? attachment = default) => context.Status(nameof(Faulted), details, attachment);
    
    internal static void Ended(this LoggerContext context, object? details = default, object? attachment = default) => context.Status(nameof(Ended), details, attachment);
}