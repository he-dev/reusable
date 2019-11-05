using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNull : ScalarExtension<bool>
    {
        public IsNull(ILogger<IsNull> logger) : base(logger, nameof(IsNull)) { }
        
        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var value = Left.Invoke(TODO).Value;
            return (Name, value is null);
        }
    }
}