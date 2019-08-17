using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public class RequestDelegateBuilder
    {
        private readonly IServiceProvider _services;

        private readonly Stack<(Type MiddlewareType, object[] Parameters)> _middleware = new Stack<(Type MiddlewareType, object[] Parameters)>();

        public RequestDelegateBuilder(IServiceProvider services)
        {
            _services = services;
        }

        public RequestDelegateBuilder UseMiddleware<T>(params object[] args)
        {
            _middleware.Push((typeof(T), args));
            return this;
        }

        public RequestDelegate<TContext> Build<TContext>()
        {
            if (!_middleware.Any())
            {
                throw new InvalidOperationException($"Cannot build {nameof(RequestDelegate<TContext>)} because there are no middleware.");
            }

            var previous = default(object);
            while (_middleware.Any())
            {
                var current = _middleware.Pop();
                var nextCallback = CreateNext<TContext>(previous);
                var parameters = new object[] { nextCallback };
                if (current.Parameters.Any())
                {
                    parameters = parameters.Concat(current.Parameters).ToArray();
                }

                var middlewareCtor = current.MiddlewareType.GetConstructor(parameters.Select(p => p.GetType()).ToArray());
                if (middlewareCtor is null)
                {
                    throw DynamicException.Create
                    (
                        "ConstructorNotFound",
                        $"Type '{current.MiddlewareType.ToPrettyString()}' does not have a constructor with these parameters: [{parameters.Select(p => p.GetType().ToPrettyString()).Join(", ")}]"
                    );
                }

                previous = middlewareCtor.Invoke(parameters);
            }

            return CreateNext<TContext>(previous);
        }

        // Using this helper to "catch" the "previous" middleware before it goes out of scope and is overwritten by the loop.
        private RequestDelegate<TContext> CreateNext<TContext>(object middleware)
        {
            // This is the last last middleware and there is nowhere to go from here.
            if (middleware is null)
            {
                return _ => Task.CompletedTask;
            }

            var invokeMethods = new[]
            {
                middleware.GetType().GetMethod("InvokeAsync"),
                middleware.GetType().GetMethod("Invoke")
            };

            var nextInvokeMethod = invokeMethods.Where(Conditional.IsNotNull).SingleOrThrow
            (
                onEmpty: () => DynamicException.Create("InvokeNotFound", $"{middleware.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke'."),
                onMany: () => DynamicException.Create("AmbiguousInvoke", $"{middleware.GetType().ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke' but not both.")
            );

            var parameters = nextInvokeMethod.GetParameters();

            if (parameters.First().ParameterType != typeof(TContext))
            {
                throw DynamicException.Create
                (
                    "InvokeSignature",
                    $"{middleware.GetType().ToPrettyString()} Invoke(Async)'s first parameters must be of type '{typeof(RequestDelegate<TContext>).ToPrettyString()}'."
                );
            }

            return context =>
            {
                var parameterValues =
                    parameters
                        .Skip(1) // TContext is always there.
                        .Select(parameter => _services.Resolve(parameter.ParameterType)) // Resolve other Invoke(Async) parameters.
                        .Prepend(context);

                // Call the actual invoke with its parameters.
                return (Task)nextInvokeMethod.Invoke(middleware, parameterValues.ToArray());
            };
            //return next.CreateDelegate<RequestCallback<TContext>>(middleware);
        }
    }

    public static class MethodInfoExtensions
    {
        public static T CreateDelegate<T>(this MethodInfo method, object target) where T : Delegate
        {
            return (T)method.CreateDelegate(typeof(T), target);
        }
    }
}