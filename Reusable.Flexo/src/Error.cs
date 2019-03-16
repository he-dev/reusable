using JetBrains.Annotations;
using Reusable.Exceptionizer;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Error : Expression
    {
        public Error(string name) : base(name) { }

        public string Message { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            throw DynamicException.Create(Name, Message);
        }
    }
}