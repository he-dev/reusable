using System;

// ReSharper disable InconsistentNaming

namespace Reusable.Marbles;

public static partial class Conditional
{
    public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TValue, TResult> ifTrue, Func<TValue, TResult> ifFalse)
    {
        return predicate(value) ? ifTrue(value) : ifFalse(value);
    }

    public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TValue, TResult> ifTrue)
    {
        return value.IIf(predicate, ifTrue, x => default!);
    }

    public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TResult> ifTrue, Func<TResult> ifFalse)
    {
        return value.IIf(predicate, x => ifTrue(), x => ifFalse());
    }

    public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TResult> ifTrue)
    {
        return value.IIf(predicate, x => ifTrue(), x => default!);
    }

    public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, TResult ifTrue, TResult ifFalse)
    {
        return value.IIf(predicate, x => ifTrue, x => ifFalse);
    }

    public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, TResult ifTrue)
    {
        return value.IIf(predicate, x => ifTrue, x => default!);
    }

    public static void IIf<TValue>(this TValue value, Func<TValue, bool> predicate, Action<TValue> ifTrue, Action<TValue> ifFalse)
    {
        if (predicate(value))
        {
            ifTrue(value);
        }
        else
        {
            ifFalse(value);
        }
    }

    public static void IIf<TValue>(this TValue value, Func<TValue, bool> predicate, Action<TValue> ifTrue)
    {
        value.IIf(predicate, ifTrue, x => { });
    }
}