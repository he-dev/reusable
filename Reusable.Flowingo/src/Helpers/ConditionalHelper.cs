using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gems.Easyflow.Helpers
{
    public static class ConditionalHelper
    {
        [DebuggerStepThrough]
        public static ISwitchBuilder<T> Switch<T>(this IEnumerable<T> source)
        {
            return new SwitchBuilder<T>(source);
        }

        [DebuggerStepThrough]
        // ReSharper disable once InconsistentNaming - this is a well known name
        public static IThenBuilder<T> IIf<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            return new IIfBuilder<T>(source, predicate);
        }
    }

    // ReSharper disable once InconsistentNaming - this is a well known name
    // public static class IIfBuilderExtensions
    // {
    //     public static Expression<TResult> Else<TSource, TResult>(this IElseBuilder<TSource, TResult> elseBuilder, IEnumerable<TResult> expression)
    //     {
    //         return elseBuilder.Else(x => Collection.Create(nameof(Else), expression, x.Context));
    //     }
    // }
}