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
    [PublicAPI]
    public class RequestDelegateBuilder<TContext>
    {
        public static readonly IImmutableList<string> InvokeMethodNames = ImmutableList<string>.Empty.Add("InvokeAsync").Add("Invoke");

        private readonly IServiceProvider _services;

        private readonly Stack<(Type Type, ConstructorInfo Ctor, MethodInfo InvokeMethod, object[] Args)> _pipeline = new Stack<(Type, ConstructorInfo, MethodInfo, object[])>();

        public RequestDelegateBuilder(IServiceProvider services)
        {
            _services = services;
        }

        public RequestDelegateBuilder<TContext> UseMiddleware(Type middlewareType, params object[] args)
        {
            _pipeline.Push((middlewareType, GetConstructor(middlewareType), GetInvokeMethod(middlewareType), args));
            var last = _pipeline.Peek();

            if (args.Any())
            {
                var ctorParameterCountWithoutContext = last.Ctor.GetParameters().Length - 1;
                if (ctorParameterCountWithoutContext != args.Length)
                {
                    throw new ArgumentException
                    (
                        paramName: nameof(args),
                        message: $"Invalid number of arguments ({args.Length} of {ctorParameterCountWithoutContext}) specified for '{middlewareType.ToPrettyString()}' #{_pipeline.Count}."
                    );
                }
            }

            return this;
        }

        public RequestDelegateBuilder<TContext> UseMiddleware<TMiddleware>(params object[] args)
        {
            return UseMiddleware(typeof(TMiddleware), args);
        }

        public RequestDelegate<TContext> Build()
        {
            if (!_pipeline.Any())
            {
                throw new InvalidOperationException($"Cannot build {nameof(RequestDelegate<TContext>)} because there are no middleware.");
            }

            var first = _pipeline.Aggregate((middleware: default(object), invokeMethod: default(MethodInfo)), (previous, current) =>
            {
                var nextCallback = CreateRequestDelegate(previous.middleware, previous.invokeMethod);
                var parameterValues = CreateConstructorParameters(current.Ctor, nextCallback, current.Args, _services);
                var middleware = current.Ctor.Invoke(parameterValues);
                return (middleware, current.InvokeMethod);
            });

            return CreateRequestDelegate(first.middleware, first.invokeMethod);
        }

        private static object[] CreateConstructorParameters(ConstructorInfo ctor, RequestDelegate<TContext> nextCallback, object[] args, IServiceProvider services)
        {
            var parameterValues =
                args.Any()
                    ? args
                    // TContext is always there so we can skip it.
                    : ctor.GetParameters().Skip(1).Select(parameter => services.Resolve(parameter.ParameterType));

            return parameterValues.Prepend(nextCallback).ToArray();
        }

        private static ConstructorInfo GetConstructor(Type middlewareType)
        {
            var ctors =
                from ctor in middlewareType.GetConstructors()
                let parameters = ctor.GetParameters()
                where parameters.Any() && typeof(RequestDelegate<TContext>).IsAssignableFrom(ctor.GetParameters().First().ParameterType)
                select ctor;

            return ctors.SingleOrThrow
            (
                onEmpty: ("ConstructorNotFound", $"Type '{middlewareType.ToPrettyString()}' does not have a constructor with the first parameter '{typeof(RequestDelegate<TContext>).ToPrettyString()}'."),
                onMany: ("AmbiguousConstructorsFound", $"Type '{middlewareType.ToPrettyString()}' has more than one constructor with the first parameter '{typeof(RequestDelegate<TContext>).ToPrettyString()}'.")
            );
        }

        // Using this helper to "catch" the "previous" middleware before it goes out of scope and is overwritten by the loop.
        private RequestDelegate<TContext> CreateRequestDelegate(object? middleware, MethodInfo? invokeMethod)
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
                        from p in invokeMethod.GetParameters().Skip(1) // TContext is always there.
                        select _services.Resolve(p.ParameterType); // Resolve other Invoke(Async) parameters.

                    // Call the actual invoke with its parameters.
                    return (Task)invokeMethod.Invoke(middleware, parameterValues.Prepend(context).ToArray());
                };
            }
        }

        private static MethodInfo GetInvokeMethod(Type middlewareType)
        {
            var invokeMethods =
                from n in InvokeMethodNames
                let m = middlewareType.GetMethod(n)
                where m.IsNotNull()
                select m;


            var invokeMethod = invokeMethods.SingleOrThrow
            (
                onEmpty: ("InvokeNotFound", $"{middlewareType.ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke'."),
                onMany: ("AmbiguousInvoke", $"{middlewareType.ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke' but not both.")
            );

            if (!typeof(Task).IsAssignableFrom(invokeMethod.ReturnType))
            {
                throw DynamicException.Create
                (
                    "InvokeSignature",
                    $"{middlewareType.ToPrettyString()} Invoke's return type must be '{typeof(Task).ToPrettyString()}'."
                );
            }

            if (!typeof(TContext).IsAssignableFrom(invokeMethod.GetParameters().FirstOrDefault()?.ParameterType))
            {
                throw DynamicException.Create
                (
                    "InvokeSignature",
                    $"{middlewareType.ToPrettyString()} Invoke's first parameters must be of type '{typeof(TContext).ToPrettyString()}'."
                );
            }

            return invokeMethod;
        }

        private static void ValidateConstructorArgs(ConstructorInfo ctor, object[] args) { }
    }


    //            var previous = (middleware: default(object), invokeMethod: default(MethodInfo));
//            while (_pipeline.Any())
//            {
//                var current = _pipeline.Pop();
//                var nextCallback = CreateRequestDelegate(previous.middleware, previous.invokeMethod);
//                var parameterValues = CreateConstructorParameters(current.Ctor, nextCallback, current.Args, _services);
//                var middleware = current.Ctor.Invoke(parameterValues);
//                previous = (middleware, current.InvokeMethod);
//            }

//    public static class MethodInfoExtensions
//    {
//        public static T CreateDelegate<T>(this MethodInfo method, object target) where T : Delegate
//        {
//            return (T)method.CreateDelegate(typeof(T), target);
//        }
//    }
}