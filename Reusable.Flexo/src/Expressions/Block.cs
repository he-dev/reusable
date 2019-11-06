using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Block : Expression<object>
    {
        public Block([NotNull] ILogger<Block> logger) : base(logger, nameof(Block)) { }

        public IEnumerable<IExpression> Body { get; set; }

        protected override Constant<object> InvokeAsConstant(IImmutableContainer context)
        {
            using (var e = Body.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    throw DynamicException.Create("EmptyBlockBody", "Block's Body has to contain at least one element.");
                }

                var last = e.Current.Invoke(context);
                while (e.MoveNext())
                {
                    last = e.Current.Invoke(context);
                }

                return (last.Name, last.Value);
            }
        }
    }
    
    public class Package : Expression
    {
        public Package([NotNull] ILogger<Package> logger) : base(logger, nameof(Block)) { }

        public IExpression Body { get; set; }

        public override IConstant Invoke(IImmutableContainer context)
        {
            return Body.Invoke(context);
        }
    }
    
    public class Import : GetItem<IExpression>
    {
        public Import([NotNull] ILogger<Ref> logger) : base(logger, nameof(Ref)) { }

        public List<string> Tags { get; set; }

        protected override Constant<IExpression> InvokeAsConstant(IImmutableContainer context)
        {
            var expressions = context.GetItemOrDefault(ExpressionContext.References);
            var path = Path.StartsWith("R.", StringComparison.OrdinalIgnoreCase) ? Path : $"R.{Path}";

            return
                expressions.TryGetValue(path, out var expression)
                    ? (Path, expression)
                    : throw DynamicException.Create("RefNotFound", $"Could not find a reference to '{path}'.");
        }
    }
}