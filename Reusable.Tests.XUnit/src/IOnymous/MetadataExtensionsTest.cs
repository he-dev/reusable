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
            var metadata = Metadata.Empty.Scope<MetadataExtensionsTest>(scope => scope.Greeting("Hi!"));
            Assert.Equal("Hi!", metadata.Scope<MetadataExtensionsTest>().Greeting());
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
        public static string Greeting(this Metadata<MetadataExtensionsTest> scope)
        {
            return scope.Value.GetItemByCallerName("Hallo!");
        }
        
        public static Metadata<MetadataExtensionsTest> Greeting(this Metadata<MetadataExtensionsTest> scope, string greeting)
        {
            return scope.Value.SetItemByCallerName(greeting);
        }
    }
}