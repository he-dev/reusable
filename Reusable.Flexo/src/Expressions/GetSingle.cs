using Reusable.Data;

namespace Reusable.Flexo
{
    public class GetSingle : GetItem<object>
    {
        public GetSingle() : base(default) { }

        protected override Constant<object> ComputeConstantGeneric(IImmutableContainer context)
        {
            return (Path, FindItem(context), context);
        }
    }
}