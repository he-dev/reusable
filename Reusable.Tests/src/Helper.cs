using Reusable.IOnymous;

namespace Reusable
{
    public static class TestHelper
    {
        public static readonly IResourceProvider Resources = new EmbeddedFileProvider(typeof(TestHelper).Assembly, @"Reusable\res");
    }
}