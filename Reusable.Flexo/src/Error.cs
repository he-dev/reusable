using JetBrains.Annotations;
using Reusable.Exceptionizer;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Error : Expression
    {
        public Error(SoftString name) : base(name) { }

        public string Message { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            throw DynamicException.Create(Name.ToString(), Message);
        }
    }
}