using System;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class LoggerContextExtensions
{
    public static LoggerContext OptIn<T>(this LoggerContext context) where T : IChannel
    {
        context
            .Properties
            .Transient
            .Set(nameof(Channel), typeof(T))
            .Set(nameof(Opt), Opt.In);
        return context;
    }

    public static LoggerContext OptOut<T>(this LoggerContext context) where T : IChannel
    {
        context
            .Properties
            .Transient
            .Set(nameof(Channel), typeof(T))
            .Set(nameof(Opt), Opt.Out);
        return context;
    }

    public static LoggerContext AttachCorrelationId(this LoggerContext context, object correlationId)
    {
        context.First().Properties.Scoped.Set(nameof(correlationId), correlationId);
        return context;
    }

    public static LoggerContext Exception(this LoggerContext context, Exception exception)
    {
        context.First().Properties.Scoped.Set(nameof(Exception), exception);
        return context;
    }
}