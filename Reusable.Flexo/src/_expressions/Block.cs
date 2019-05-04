using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Block : Expression<object>
    {
        public Block([NotNull] ILogger<Block> logger) : base(logger, nameof(Block)) { }

        public IEnumerable<IExpression> Body { get; set; }

        protected override Constant<object> InvokeCore()
        {
            using (var e = Body.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    throw DynamicException.Create("EmptyBlockBody", "Block's Body has to contain at least one element.");
                }

                var last = e.Current.Invoke();
                while (e.MoveNext())
                {
                    last = e.Current.Invoke();
                }

                return (last.Name, last.Value);
            }
        }
    }
}