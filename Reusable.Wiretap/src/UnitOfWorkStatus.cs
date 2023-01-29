using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Marbles;

namespace Reusable.Wiretap;

public static class UnitOfWorkStatus
{
    internal static void Started(this UnitOfWork.Context context, object? details = default, object? attachment = default)
    {
        context.Status(nameof(Started), details, attachment);
    }

    public static void Running(this UnitOfWork.Context context, object? details = default, object? attachment = default)
    {
        context.Status(nameof(Running), details, attachment);
    }

    public static void Completed(this UnitOfWork.Context context, object? details = default, object? attachment = default)
    {
        context.First().Set(nameof(Completed), true);
        context.Status(nameof(Completed), details, attachment);
    }

    public static void Canceled(this UnitOfWork.Context context, object? details = default, object? attachment = default)
    {
        context.First().Set(nameof(Canceled), true);
        context.Status(nameof(Canceled), details, attachment);
    }

    public static void Faulted(this UnitOfWork.Context context, object? details = default, object? attachment = default)
    {
        context.First().Set(nameof(Faulted), true);
        context.Status(nameof(Faulted), details, attachment);
    }

    internal static void Inconclusive(this UnitOfWork.Context context, object? details = default, object? attachment = default)
    {
        context.Status(nameof(Inconclusive), details, attachment);
    }

    public static void Commit(this UnitOfWork.Context context, object? details = default, object? attachment = default)
    {
        context.First().Also(cache =>
        {
            cache.Set(nameof(Commit), true);
            cache.Set(nameof(details), details);
            cache.Set(nameof(attachment), attachment);
        });
    }
}