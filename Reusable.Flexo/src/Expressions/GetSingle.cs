using Reusable.Data;

namespace Reusable.Flexo
{
    public class GetSingle : GetItem<object>
    {
        public GetSingle() : base(default, nameof(GetSingle)) { }

        protected override Constant<object> ComputeConstantGeneric(IImmutableContainer context)
        {
            return (Path, FindItem(context), context);
        }
    }
}