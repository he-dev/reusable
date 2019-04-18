using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class ResourceMetadataExtensionsTest
    {
        [Fact]
        public void Can_set_scope()
        {
            var metadata = Metadata.Empty.Scope<ResourceMetadataExtensionsTest>(scope => scope.Greeting("Hi!"));
            Assert.Equal("Hi!", metadata.Scope<ResourceMetadataExtensionsTest>().Greeting());
        }
    }

    internal static class TestExtensions
    {
        public static string Greeting(this MetadataScope<ResourceMetadataExtensionsTest> scope)
        {
            return scope.Metadata.GetValueOrDefault("Hallo!");
        }
        
        public static MetadataScope<ResourceMetadataExtensionsTest> Greeting(this MetadataScope<ResourceMetadataExtensionsTest> scope, string greeting)
        {
            return scope.Metadata.SetItemAuto(greeting);
        }
    }
}