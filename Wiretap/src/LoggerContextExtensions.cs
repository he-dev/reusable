using System;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class LoggerContextExtensions
{
    public static LoggerContext OptIn<T>(this LoggerContext context) where T : IChannel
    {
        context.Properties.Transient.Add(nameof(Channel), typeof(T));
        context.Properties.Transient.Add(nameof(Opt), Opt.In);
        return context;
    }

    public static LoggerContext OptOut<T>(this LoggerContext context) where T : IChannel
    {
        context.Properties.Transient.Add(nameof(Channel), typeof(T));
        context.Properties.Transient.Add(nameof(Opt), Opt.Out);
        return context;
    }

    public static LoggerContext AttachCorrelationId(this LoggerContext context, object correlationId)
    {
        context.First().Properties.Scoped.Add(nameof(correlationId), correlationId);
        return context;
    }

    public static LoggerContext Exception(this LoggerContext context, Exception exception)
    {
        context.First().Properties.Scoped.Add(nameof(Exception), exception);
        return context;
    }
}