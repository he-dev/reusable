using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class ResourceProviderTest
    {
        [Fact]
        public async Task Throws_when_unsupported_scheme_requested()
        {
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.GetAsync("noop"));            
        }
        
        [Fact]
        public async Task Throws_when_unsupported_method_requested()
        {
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.GetAsync("noop"));            
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.PutAsync("noop", Stream.Null));            
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.PostAsync("noop", Stream.Null));            
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.DeleteAsync("noop"));            
        }

        private class EmptyProvider : ResourceProvider
        {
            public EmptyProvider() : base(new SoftString[] { "test" }, Metadata.Empty) { }
            
            public static EmptyProvider Default => new EmptyProvider();
        }

        private class SimpleProvider : ResourceProvider
        {
            public SimpleProvider() : base(new SoftString[] { "test" }, Metadata.Empty) { }
            
            public static SimpleProvider Default => new SimpleProvider();

            protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata)
            {
                return base.GetAsyncInternal(uri, metadata);
            }

            protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, Metadata metadata)
            {
                return base.PutAsyncInternal(uri, value, metadata);
            }

            protected override Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata)
            {
                return base.PostAsyncInternal(uri, value, metadata);
            }

            protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, Metadata metadata)
            {
                return base.DeleteAsyncInternal(uri, metadata);
            }
        }
    }
}