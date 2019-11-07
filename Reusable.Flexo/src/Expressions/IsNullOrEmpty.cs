using Reusable.Data;

namespace Reusable.Flexo
{
    public class IsNullOrEmpty : ScalarExtension<bool>
    {
        public IsNullOrEmpty() : base(default, nameof(IsNullOrEmpty)) { }

        public IExpression Left
        {
            get => ThisInner;
            set => ThisInner = value;
        }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return This(context).Invoke(context).Value switch { string s => string.IsNullOrEmpty(s), _ => true };
        }
    }
}