using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Throw : Expression<IExpression>
    {
        public Throw() : base(default) { }

        public IExpression Message { get; set; }

        protected override IExpression ComputeValue(IImmutableContainer context)
        {
            throw DynamicException.Create(Id.ToString(), Message.Invoke(context).Value<string>());
        }
    }
}