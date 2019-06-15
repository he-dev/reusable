using System.IO;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.IOnymous
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
            Assert.False(EmptyProvider.Default.CanGet);
            Assert.False(EmptyProvider.Default.CanPut);
            Assert.False(EmptyProvider.Default.CanPost);
            Assert.False(EmptyProvider.Default.CanDelete);
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.GetAsync("noop"));            
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.PutAsync("noop", Stream.Null));            
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.PostAsync("noop", Stream.Null));            
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await EmptyProvider.Default.DeleteAsync("noop"));            
        }

        [Fact]
        public async Task Throws_when_resource_format_not_specified()
        {
            Assert.True(SimpleProvider.Default.CanGet);
            await SimpleProvider.Default.GetAsync("test:///noop");
        }

        private class EmptyProvider : ResourceProvider
        {
            public EmptyProvider() : base(new SoftString[] { "test" }, ImmutableSession.Empty) { }
            
            public static EmptyProvider Default => new EmptyProvider();
        }

        private class SimpleProvider : ResourceProvider
        {
            public SimpleProvider() : base(new SoftString[] { "test" }, ImmutableSession.Empty) { }
            
            public static SimpleProvider Default => new SimpleProvider();

            protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
            {
                return Task.FromResult(default(IResourceInfo));
            }

            protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
            {
                return Task.FromResult(default(IResourceInfo));
            }

            protected override Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
            {
                return Task.FromResult(default(IResourceInfo));            }

            protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
            {
                return Task.FromResult(default(IResourceInfo));
            }
        }
    }
}