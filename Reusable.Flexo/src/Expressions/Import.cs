using System;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    public class Import : Expression
    {
        public Import() : base(default, nameof(Import)) { }

        public string Package { get; set; }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            if (string.IsNullOrEmpty(Package)) throw new InvalidOperationException($"{nameof(Package)} must be not null or empty.");

            foreach (var packages in context.FindItems(ExpressionContext.Packages))
            {
                if (packages.TryGetValue(Package, out var package))
                {
                    return package.Invoke(context);
                }
            }

            throw DynamicException.Create("PackageNotFound", $"Could not import package '{Package}'.");
        }
    }
}