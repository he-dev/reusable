using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class UnitOfWorkExtensions
{
    public static UnitOfWork.Context OptIn<T>(this UnitOfWork.Context context) where T : IChannel
    {
        context.OneTimeData[nameof(Channel)] = typeof(T);
        context.OneTimeData[nameof(Opt)] = Opt.In;
        return context;
    }

    public static UnitOfWork.Context OptOut<T>(this UnitOfWork.Context context) where T : IChannel
    {
        context.OneTimeData[nameof(Channel)] = typeof(T);
        context.OneTimeData[nameof(Opt)] = Opt.Out;
        return context;
    }

    public static void Exception(this UnitOfWork.Context context, Exception exception)
    {
        context.First().Set(nameof(Exception), exception);
    }
}