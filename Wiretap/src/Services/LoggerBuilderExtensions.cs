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
    public static LoggerBuilder Use<T>(this LoggerBuilder builder, T module) where T : IModule
    {
        return builder.Also(b => b.Add(new InvokeWhen
        {
            Module = module, 
            Filter = new Modules.OptFilter()
        }));
    }

    public static LoggerBuilder Use<T>(this LoggerBuilder builder) where T : IModule, new()
    {
        return builder.Use(new T());
    }

    public static LoggerBuilder Use(this LoggerBuilder builder, Func<IActivity, LogEntry, LogEntry> middleware)
    {
        return builder.Use(new InvokeFunc(middleware));
    }

    public static LoggerBuilder Configure<T>(this LoggerBuilder builder, Action<T> configure) where T : IModule
    {
        return builder.Also(b => configure(b.OfType<T>().SingleOrThrow($"Could not find a single '{typeof(T)}'.")));
    }
}