using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNullOrEmpty : ScalarExtension<bool>
    {
        public IsNullOrEmpty(ILogger<IsNullOrEmpty> logger) : base(logger, nameof(IsNullOrEmpty)) { }
        
        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var value = (string)Left.Invoke(TODO).Value;
            return (Name, string.IsNullOrEmpty(value));
        }
    }
}