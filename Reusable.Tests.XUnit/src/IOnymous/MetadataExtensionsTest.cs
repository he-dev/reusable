using System.Linq;
using Reusable.Data;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class MetadataExtensionsTest
    {
        [Fact]
        public void Can_set_scope()
        {
            var metadata = ImmutableSession.Empty.SetItem("Global", "ABC").Set(Use<ITestSession>.Scope, x => x.Greeting, "Hi!");

            Assert.Equal(2, metadata.Count);

            Assert.True(metadata.ContainsKey("Global"));
            Assert.True(metadata.ContainsKey("TestSession.Greeting"));

            Assert.Equal("Hi!", metadata.Get(Use<ITestSession>.Scope, x => x.Greeting));
            Assert.Equal("ABC", metadata["Global"]);
        }

//        [Fact]
//        public void Union_can_merge_two_metadata()
//        {
//            // Makes sure that nested metadata is updated properly.
//
//            var m = Metadata.Empty.Scope<IResourceMetadata>(s => s.Set(x => x.ActualName, "Bob"));
//            var c = new ConfigureMetadataScopeCallback<IResourceInfo>(s => s.Format(MimeType.Text).Union(m));
//
//            // Tests the pattern used by the ResourceInfo to merge outer metadata.
//            m = m.Resource(s => c(s));
//
//            Assert.Equal(1, m.Keys.Count());
//        }

        [Fact]
        public void Union_can_merge_two_metadata_and_scope() { }
    }

    internal interface ITestSession : ISession
    {
        string Greeting { get; }
    }
}