using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Throw : Expression<IExpression>
    {
        public Throw(SoftString name) : base(name) { }

        //public string Exception { get; set; }
        
        public IExpression Message { get; set; }

        protected override Constant<IExpression> InvokeCore(IExpressionContext context)
        {
            throw DynamicException.Create(Name.ToString(), Message.Invoke(context).Value<string>());
        }
    }
}