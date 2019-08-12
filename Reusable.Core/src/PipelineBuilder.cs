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

    public class PipelineBuilder
    {
        private readonly Stack<Type> _middlewareTypes = new Stack<Type>();
        private readonly ILifetimeScope _lifetimeScope;

        public PipelineBuilder(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public PipelineBuilder Add<T>()
        {
            _middlewareTypes.Push(typeof(T));
            return this;
        }

        public RequestCallback<TContext> Build<TContext>()
        {
            var previous = default(object);
            while (_middlewareTypes.Any())
            {
                var middlewareType = _middlewareTypes.Pop();
                var next = CreateNext<TContext>(previous);
                previous = _lifetimeScope.Resolve(middlewareType, new TypedParameter(typeof(RequestCallback<TContext>), next));
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