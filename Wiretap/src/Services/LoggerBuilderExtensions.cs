using System;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Services;

public static class LoggerBuilderExtensions
{
    public static LoggerBuilder Use<T>(this LoggerBuilder builder, T middleware) where T : IMiddleware
    {
        return builder.Also(b => b.Add(middleware));
    }

    public static LoggerBuilder Use<T>(this LoggerBuilder builder) where T : IMiddleware, new()
    {
        return builder.Use(new T());
    }
    
    public static LoggerBuilder Use(this LoggerBuilder builder, Func<LogEntry, LogEntry> middleware)
    {
        return builder.Use(new AttachFunc(middleware));
    }

    public static LoggerBuilder Configure<T>(this LoggerBuilder builder, Action<T> configure) where T : IMiddleware
    {
        return builder.Also(b => configure(b.OfType<T>().SingleOrThrow($"Could not find a single '{typeof(T)}'.")));
    }
}