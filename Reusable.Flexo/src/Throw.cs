using JetBrains.Annotations;
using Reusable.Exceptionizer;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Throw : Expression
    {
        public Throw(SoftString name) : base(name) { }

        //public string Exception { get; set; }
        
        public string Message { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            throw DynamicException.Create(Name.ToString(), Message);
        }
    }
}