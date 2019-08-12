using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
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
        private RequestCallback<TContext> CreateNext<TContext>(object previous)
        {
            // This is the last last middleware. There is nowhere to go from here.
            if (previous is null)
            {
                return _ => Task.CompletedTask;
            }

            // Doing it here to avoid reflection next time.
            var nextInvokeAsync = previous.GetType().GetMethod("InvokeAsync");
            var nextInvoke = previous.GetType().GetMethod("Invoke");

            if (nextInvokeAsync is null && nextInvoke is null)
            {
                throw DynamicException.Create("InvokeNotFound", $"{previous.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke'.");
            }

            if (!(nextInvokeAsync is null) && !(nextInvoke is null))
            {
                throw DynamicException.Create("AmbiguousInvoke", $"{previous.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke' but not both.");
            }

            var next = nextInvokeAsync ?? nextInvoke;

            return context => (Task)next.Invoke(previous, new object[] { context });
        }
    }
}