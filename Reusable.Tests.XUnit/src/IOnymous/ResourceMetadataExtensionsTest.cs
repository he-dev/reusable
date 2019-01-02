using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class ResourceMetadataExtensionsTest
    {
        [Fact]
        public void Can_set_scope()
        {
            var metadata = ResourceMetadata.Empty.Scope<ResourceMetadataExtensionsTest>(scope => scope.Greeting("Hi!"));
            Assert.Equal("Hi!", metadata.Scope<ResourceMetadataExtensionsTest>().Greeting());
        }
    }

    internal static class TestExtensions
    {
        public static string Greeting(this ResourceMetadataScope<ResourceMetadataExtensionsTest> scope)
        {
            return scope.Metadata.GetValueOrDefault("Hallo!");
        }
        
        public static ResourceMetadataScope<ResourceMetadataExtensionsTest> Greeting(this ResourceMetadataScope<ResourceMetadataExtensionsTest> scope, string greeting)
        {
            return scope.Metadata.SetItemSafe(greeting);
        }
    }
}