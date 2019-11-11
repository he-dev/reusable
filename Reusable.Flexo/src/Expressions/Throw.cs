using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Throw : Expression<IExpression>
    {
        public Throw() : base(default) { }

        public string? Exception { get; set; }
        
        public string Message { get; set; }

        protected override IExpression ComputeValue(IImmutableContainer context)
        {
            throw DynamicException.Create(Exception ?? Id.ToString(), Message);
        }
    }
}