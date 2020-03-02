using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class Functional
    {
        /// <summary>
        /// Allows to pipe an action on the current object in a functional way.
        /// </summary>
        public static T Pipe<T>(this T obj, Action<T>? next)
        {
            next?.Invoke(obj);
            return obj;
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
        public static TOut Map<TIn, TOut>(this TIn input, Func<TIn, TOut> pipe) => pipe(input);

        /// <summary>
        /// Returns the same value.
        /// </summary>
        [MustUseReturnValue]
        public static T Echo<T>(this T value) => value;

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
}