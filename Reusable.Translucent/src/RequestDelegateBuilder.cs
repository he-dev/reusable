using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Quickey;
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

                var middlewareCtors =
                    from ctor in current.MiddlewareType.GetConstructors()
                    where ctor.GetParameters().FirstOrDefault()?.ParameterType == nextCallback.GetType()
                    select ctor;

                var middlewareCtor = middlewareCtors.SingleOrThrow
                (
                    onEmpty: () => throw DynamicException.Create
                    (
                        "ConstructorNotFound",
                        $"Type '{current.MiddlewareType.ToPrettyString()}' does not have a constructor with the first parameter '{nextCallback.GetType().ToPrettyString()}'."
                    ),
                    onMany: () => throw DynamicException.Create
                    (
                        "MultipleConstructorsFound",
                        $"Type '{current.MiddlewareType.ToPrettyString()}' has more than one constructor with the first parameter '{nextCallback.GetType().ToPrettyString()}'."
                    )
                );

                var parameters = middlewareCtor.GetParameters(); // new object[] { nextCallback };

                var parameterValues =
                    current.Parameters.Any()
                        ? current.Parameters
                        // TContext is always there so we can skip it.
                        : parameters.Skip(1).Select(parameter => _services.Resolve(parameter.ParameterType));

                parameterValues = parameterValues.Prepend(nextCallback);

                previous = middlewareCtor.Invoke(parameterValues.ToArray());
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