using Autofac;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Utilities.Autofac
{
    public class MiddlewareBuilderWithAutofac : MiddlewareBuilder
    {
        public MiddlewareBuilderWithAutofac(IComponentContext componentContext)
        {
            Resolve =
                type =>
                    componentContext.IsRegistered(type)
                        ? componentContext.Resolve(type)
                        : throw DynamicException.Create("TypeNotFound", $"Could not resolve '{type.ToPrettyString()}'.");
        }
    }
}