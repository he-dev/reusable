using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Translucent
{
    public static class PipelineFactory
    {
        public static RequestDelegate<TContext> CreatePipeline<TContext>(IEnumerable<IMiddlewareInfo<TContext>> middleware, IServiceProvider services)
        {
            var pipeline = new Stack<IMiddlewareInfo<TContext>>(middleware);

            if (!pipeline.Any())
            {
                throw new InvalidOperationException($"Cannot build {nameof(RequestDelegate<TContext>)} because there are no middleware.");
            }

            var first = pipeline.Aggregate((instance: default(object), invokeMethod: default(MethodInfo)), (previous, current) =>
            {
                var nextCallback = CreateRequestDelegate<TContext>(previous.instance, previous.invokeMethod, services);
                var ctor = current.GetConstructor();
                var parameterValues = CreateConstructorParameters(ctor, nextCallback, current.Args, services);
                var instance = ctor.Invoke(parameterValues);
                return (instance, current.GetInvokeMethod());
            });

            return CreateRequestDelegate<TContext>(first.instance, first.invokeMethod, services);
        }

        private static object[] CreateConstructorParameters<TContext>(ConstructorInfo ctor, RequestDelegate<TContext> nextCallback, object[] args, IServiceProvider services)
        {
            var parameterValues =
                args?.Any() == true
                    ? args
                    // TContext is always there so we can skip it.
                    : ctor.GetParameters().Skip(1).Select(parameter => services.Resolve(parameter.ParameterType));

            return parameterValues.Prepend(nextCallback).ToArray();
        }

        // Using this helper to "catch" the "previous" middleware before it goes out of scope and is overwritten by the loop.
        private static RequestDelegate<TContext> CreateRequestDelegate<TContext>(object? middleware, MethodInfo? invokeMethod, IServiceProvider services)
        {
            // This is the last last middleware and there is nowhere to go from here.
            if (middleware is null)
            {
                return _ => Task.CompletedTask;
            }
            else
            {
                return context =>
                {
                    var parameterValues =
                        from p in invokeMethod!.GetParameters().Skip(1) // TContext is always there.
                        select services.Resolve(p.ParameterType); // Resolve other Invoke(Async) parameters.

                    // Call the actual invoke with its parameters.
                    return (Task)invokeMethod.Invoke(middleware, parameterValues.Prepend(context).ToArray());
                };
            }
        }
    }
}