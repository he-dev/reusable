using System.Linq;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class MetadataExtensionsTest
    {
        [Fact]
        public void Can_set_scope()
        {
            var metadata = Metadata.Empty.For<MetadataExtensionsTest>(scope => scope.Greeting("Hi!"));
            Assert.Equal("Hi!", metadata.For<MetadataExtensionsTest>().Greeting());
        }

        [Fact]
        public void Union_can_merge_two_metadata()
        {
            // Makes sure that nested metadata is updated properly.
            
            var m = Metadata.Empty.Resource(s => s.InternalName("Bob"));
            var c = new ConfigureMetadataScopeCallback<IResourceInfo>(s => s.Format(MimeType.Text).Union(m));
            
            // Tests the pattern used by the ResourceInfo to merge outer metadata.
            m = m.Resource(s => c(s));
            
            Assert.Equal(1, m.Keys.Count());
        }
        
        [Fact]
        public void Union_can_merge_two_metadata_and_scope()
        {
            
        }
    }

    internal static class TestExtensions
    {
        public static string Greeting(this MetadataScope<MetadataExtensionsTest> scope)
        {
            return scope.Metadata.GetValueByCallerName("Hallo!");
        }
        
        public static MetadataScope<MetadataExtensionsTest> Greeting(this MetadataScope<MetadataExtensionsTest> scope, string greeting)
        {
            return scope.Metadata.SetItemWithCallerName(greeting);
        }
    }
}