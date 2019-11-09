using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    // There is already an ExpressionExtension so you use Helpers to easier tell them apart. 
    public static partial class ExpressionExtensions
    {
        /// <summary>
        /// Invokes enabled expressions.
        /// </summary>
        public static IEnumerable<IConstant> Invoke(this IEnumerable<IExpression> expressions, IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return
                from expression in expressions.Enabled()
                select expression.Invoke(context, scope);
        }

        public static IConstant Invoke(this IExpression expression, IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return expression.Invoke(context.BeginScope(scope));
        }

        public static IConstant InvokePackage(this IImmutableContainer context, string packageId, IImmutableContainer? scope = default)
        {
            foreach (var packages in context.FindItems(ExpressionContext.Packages))
            {
                if (packages.TryGetValue(packageId, out var package))
                {
                    return package.Invoke(context, scope);
                }
            }

            throw DynamicException.Create("PackageNotFound", $"Could not find package '{packageId}'.");
        }
    }
}