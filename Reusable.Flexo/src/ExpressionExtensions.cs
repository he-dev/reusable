using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    // There is already an ExpressionExtension so you use Helpers to easier tell them apart. 
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets only enabled expressions.
        /// </summary>
        public static IEnumerable<T> Enabled<T>(this IEnumerable<T> expressions) where T : ISwitchable
        {
            return
                from expression in expressions
                where expression.Enabled
                select expression;
        }

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
        
//        public static IConstant<T> Invoke<T>(this IExpression expression, IImmutableContainer context, IImmutableContainer? scope = default)
//        {
//            var result = expression.Invoke(context.BeginScope(scope));
//            return new Constant<T>(result.Id.ToString(), result.Cast<T>(), context);
//        }

        // public static IConstant InvokePackage(this IImmutableContainer context, string packageId, IImmutableContainer? scope = default)
        // {
        //     var tryCount = 0;
        //     foreach (var tryGetPackage in context.FindItems(ExpressionContext.TryGetPackageFunc))
        //     {
        //         tryCount++;
        //         if (tryGetPackage(packageId, out var package))
        //         {
        //             return package.Invoke(context, scope);
        //         }
        //     }
        //
        //     throw DynamicException.Create("PackageNotFound", $"Could not find package '{packageId}' after {tryCount} {(tryCount == 1 ? "try" : "tries")}.");
        // }
    }
}