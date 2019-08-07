using Reusable.Extensions;
using Reusable.IOnymous;

namespace Reusable.Flexo
{
    internal static class Helper
    {
        public static readonly IResourceProvider Flexo =
            new EmbeddedFileProvider(typeof(ExpressionFixture).Assembly, "Reusable")
                .DecorateWith<IResourceProvider>(instance => new RelativeProvider(@"res\Flexo", instance));
    }
}