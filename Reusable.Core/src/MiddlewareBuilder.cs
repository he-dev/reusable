using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable
{
    public delegate Task RequestCallback<in TContext>(TContext context);

    public class MiddlewareBuilder
    {
        private readonly Stack<(Type MiddlewareType, object[] Parameters)> _middleware = new Stack<(Type MiddlewareType, object[] Parameters)>();

        public MiddlewareBuilder Add<T>(params object[] parameters)
        {
            _middleware.Push((typeof(T), parameters));
            return this;
        }

        public RequestCallback<TContext> Build<TContext>()
        {
            var previous = default(object);
            while (_middleware.Any())
            {
                var current = _middleware.Pop();
                var next = CreateNext<TContext>(previous);
                var parameters = new object[] { next };
                if (current.Parameters.Any())
                {
                    parameters = parameters.Concat(current.Parameters).ToArray();
                    var ctor = current.MiddlewareType.GetConstructor(parameters.Select(p => p.GetType()).ToArray());
                    previous = ctor.Invoke(parameters);
                }
                else
                {
                    previous = current.MiddlewareType.GetConstructor(parameters.Select(p => p.GetType()).ToArray()).Invoke(parameters);
                }
            }

            return CreateNext<TContext>(previous);
        }

        // Using this helper to "catch" the "previous" middleware before it goes out of scope and is overwritten by the loop.
        private RequestCallback<TContext> CreateNext<TContext>(object middleware)
        {
            // This is the last last middleware. There is nowhere to go from here.
            if (middleware is null)
            {
                return _ => Task.CompletedTask;
            }

            // Doing it here to avoid reflection next time.
            var invokeAsyncMethod = middleware.GetType().GetMethod("InvokeAsync");
            var invokeMethod = middleware.GetType().GetMethod("Invoke");
            

            if (invokeAsyncMethod is null && invokeMethod is null)
            {
                throw DynamicException.Create("InvokeNotFound", $"{middleware.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke'.");
            }

            if (!(invokeAsyncMethod is null) && !(invokeMethod is null))
            {
                throw DynamicException.Create("AmbiguousInvoke", $"{middleware.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke' but not both.");
            }

            var next = invokeAsyncMethod ?? invokeMethod;

            //return context => (Task)next.Invoke(previous, new object[] { context });
            return next.CreateDelegate<RequestCallback<TContext>>(middleware);
        }
    }

    internal static class MethodInfoExtensions
    {
        public static T CreateDelegate<T>(this MethodInfo method, object target) where T : Delegate
        {
            return (T)method.CreateDelegate(typeof(T), target);
        }
    }
}