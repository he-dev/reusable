using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    [Obsolete("Use Invoke(context) extension.")]
    public interface IExpressionInvoker
    {
        IConstant Invoke(string packageId, IImmutableContainer context);
    }

    public class ExpressionInvoker : IExpressionInvoker
    {
        public static IExpressionInvoker Default { get; } = new ExpressionInvoker();

        public IConstant Invoke(string packageId, IImmutableContainer context)
        {
            return
                context.Find(ExpressionContext.Packages).TryGetValue(packageId, out var package)
                    ? package.Invoke(context)
                    : throw DynamicException.Create("PackageNotFound", $"Could not find package '{packageId}'.");
        }
    }
}