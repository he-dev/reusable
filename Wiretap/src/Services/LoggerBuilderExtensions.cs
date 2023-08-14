using System;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Modules;

namespace Reusable.Wiretap.Services;

public static class LoggerBuilderExtensions
{
    public static LogActionBuilder Use<T>(this LogActionBuilder builder, T module) where T : IModule
    {
        return builder.Also(b => b.Add(new InvokeWhen
        {
            Module = module,
            Filter = new OptFilter()
        }));
    }

    public static LogActionBuilder Use<T>(this LogActionBuilder builder) where T : IModule, new()
    {
        return builder.Use(new T());
    }

    public static LogActionBuilder Use(this LogActionBuilder builder, Action<TraceContext> invokeAction)
    {
        return builder.Use(new InvokeAction { Body = invokeAction });
    }

    public static LogActionBuilder Configure<T>(this LogActionBuilder builder, Action<T> configure) where T : IModule
    {
        return builder.Also(b => configure(b.OfType<T>().SingleOrThrow($"Could not find a single '{typeof(T)}'.")));
    }
}