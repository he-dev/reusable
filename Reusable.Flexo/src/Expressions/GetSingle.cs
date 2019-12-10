using Reusable.Data;

namespace Reusable.Flexo
{
    public class GetSingle : GetItem<object>
    {
        public GetSingle() : base(default) { }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            return Constant.Single(Path, FindItem(context), context);
        }
    }
}