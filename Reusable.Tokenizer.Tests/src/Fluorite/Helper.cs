namespace Reusable.Tests.MarkupBuilder
{
    internal class Helper
    {
        public static IResourceProvider ResourceProvider { get; } = new RelativeProvider(new EmbeddedFileProvider(typeof(Helper).Assembly), @"res\MarkupBuilder");
    }
}
