using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Extensions;

[PublicAPI]
public static class DelegateExtensions
{
    public static T Also<T>(this T obj, Action? also)
    {
        also?.Invoke();
        return obj;
    }
    
    /// <summary>
    /// Allows to pipe an action on the current object in a functional way.
    /// </summary>
    //[MustUseReturnValue]
    public static T Also<T>(this T obj, Action<T>? next)
    {
        next?.Invoke(obj);
        return obj;
    }

    //[MustUseReturnValue]
    public static T Also<T>(this T obj, Func<bool> when, Action<T>? next)
    {
        if (when())
        {
            next?.Invoke(obj);
        }

        return obj;
    }

    public static TResult Let<T, TResult>(this T obj, Func<T, TResult> next)
    {
        return next(obj);
    }

    [MustUseReturnValue]
    public static async Task<T> Pipe<T>(this T obj, Func<T, Task>? next)
    {
        await (next ?? (_ => Task.CompletedTask)).Invoke(obj);
        return obj;
    }

    /// <summary>
    /// Allows to pipe an action on the current object in a functional way and return a different object.
    /// </summary>
    public static TOut Map<TIn, TOut>(this TIn input, Func<TIn, TOut> map) => map(input);

    [MustUseReturnValue]
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        return source.Select(item => item.Also(action));
    }

    /// <summary>
    /// Returns the same value.
    /// </summary>
    [MustUseReturnValue]
    public static T Echo<T>(this T value) => value;

    /// <summary>
    /// No-operation action.
    /// </summary>
    public static Action<T> Noop<T>(this Action<T> action) => _ => { };

    /// <summary>
    /// Combines two actions into one.
    /// </summary>
    [MustUseReturnValue]
    public static Action<T> Then<T>(this Action<T> first, Action<T> second)
    {
        return x =>
        {
            first(x);
            second(x);
        };
    }
}