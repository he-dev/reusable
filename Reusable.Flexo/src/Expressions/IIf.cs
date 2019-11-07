using System;
using Reusable.Data;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : ScalarExtension<object>
    {
        public IIf() : base(default, nameof(IIf)) { }

        public IExpression Predicate { get => ThisInner; set => ThisInner = value; }

        public IExpression True { get; set; }

        public IExpression False { get; set; }

        protected override object ComputeValue(IImmutableContainer context)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");

            return Predicate.Invoke(context).Value<bool>() switch
            {
                true => True?.Invoke(context)?.Value ?? Constant.Null,
                false => False?.Invoke(context)?.Value ?? Constant.Null
            };
        }
    }
}