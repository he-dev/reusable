using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNullOrEmpty : ScalarExtension<bool>
    {
        public IsNullOrEmpty() : base(default, nameof(IsNullOrEmpty)) { }
        
        public IExpression Left { get => ThisInner; set => ThisInner = value; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            return This(context).Invoke(context).Value switch
            {
                string s => string.IsNullOrEmpty(s),
                _ => true
            };
        }
    }
}