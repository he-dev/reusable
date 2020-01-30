using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResourceRepository
    {
        Task<Response> InvokeAsync(Request request);
    }

    [PublicAPI]
    public class ResourceRepository : IResourceRepository //<TSetup> : IResourceRepository where TSetup : new()
    {
        private readonly RequestDelegate<ResourceContext> _requestDelegate;

        internal ResourceRepository(RequestDelegate<ResourceContext> requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public static ResourceRepositoryBuilder Builder() => new ResourceRepositoryBuilder();

        public static IResourceRepository From<TSetup>(IServiceProvider services) where TSetup : IResourceRepositorySetup, new()
        {
            var setup = new TSetup();

            var builder = Builder();

            foreach (var controller in setup.Controllers(services))
            {
                builder.Add(controller);
            }

            foreach (var (type, args) in setup.Middleware(services))
            {
                builder.Use(type, args);
            }

            return builder.Build(services);
        }

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestDelegate(context);

            return context.Response;
        }
    }


    public interface IMiddlewareInfo<TContext>
    {
        Type Type { get; }

        object[] Args { get; }

        ConstructorInfo GetConstructor();

        MethodInfo GetInvokeMethod();

        void Deconstruct(out Type type, out object[] args);
    }

    public class MiddlewareInfo<TContext> : IMiddlewareInfo<TContext>
    {
        // ReSharper disable once StaticMemberInGenericType - this is OK
        public static readonly IImmutableList<string> InvokeMethodNames = ImmutableList<string>.Empty.Add("InvokeAsync").Add("Invoke");
        
        public Type Type { get; set; }

        public object[] Args { get; set; }

        public static MiddlewareInfo<TContext> Create<T>(params object[] args) => new MiddlewareInfo<TContext>
        {
            Type = typeof(T),
            Args = args
        };
        
        public ConstructorInfo GetConstructor()
        {
            var ctors =
                from ctor in Type.GetConstructors()
                let parameters = ctor.GetParameters()
                where parameters.Any() && typeof(RequestDelegate<TContext>).IsAssignableFrom(ctor.GetParameters().First().ParameterType)
                select ctor;

            var match = ctors.SingleOrThrow
            (
                onEmpty: ("ConstructorNotFound", $"Type '{Type.ToPrettyString()}' does not have a constructor with the first parameter '{typeof(RequestDelegate<TContext>).ToPrettyString()}'."),
                onMany: ("AmbiguousConstructorsFound", $"Type '{Type.ToPrettyString()}' has more than one constructor with the first parameter '{typeof(RequestDelegate<TContext>).ToPrettyString()}'.")
            );
            
            if (Args.Any())
            {
                var ctorParameterCountWithoutDelegate = match.GetParameters().Length - 1;
                if (ctorParameterCountWithoutDelegate != Args.Length)
                {
                    throw new ArgumentException
                    (
                        paramName: nameof(Args),
                        message: $"Invalid number of arguments ({Args.Length} of {ctorParameterCountWithoutDelegate}) specified for '{Type.ToPrettyString()}'."
                    );
                }
            }

            return match;
        }
        
        public MethodInfo GetInvokeMethod()
        {
            var invokeMethods =
                from n in InvokeMethodNames
                let m = Type.GetMethod(n)
                where m.IsNotNull()
                select m;


            var invokeMethod = invokeMethods.SingleOrThrow
            (
                onEmpty: ("InvokeNotFound", $"{Type.ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke'."),
                onMany: ("AmbiguousInvoke", $"{Type.ToPrettyString()} must implement either 'InvokeAsync' or 'Invoke' but not both.")
            );

            if (!typeof(Task).IsAssignableFrom(invokeMethod.ReturnType))
            {
                throw DynamicException.Create
                (
                    "InvokeSignature",
                    $"{Type.ToPrettyString()} Invoke's return type must be '{typeof(Task).ToPrettyString()}'."
                );
            }

            if (!typeof(TContext).IsAssignableFrom(invokeMethod.GetParameters().FirstOrDefault()?.ParameterType))
            {
                throw DynamicException.Create
                (
                    "InvokeSignature",
                    $"{Type.ToPrettyString()} Invoke's first parameters must be of type '{typeof(TContext).ToPrettyString()}'."
                );
            }

            return invokeMethod;
        }

        public void Deconstruct(out Type type, out object[] args)
        {
            type = Type;
            args = Args;
        }
    }
}