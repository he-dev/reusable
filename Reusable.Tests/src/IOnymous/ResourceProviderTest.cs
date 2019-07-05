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
            var provider = new EmptyProvider();
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await provider.GetAsync("noop", ImmutableContainer.Empty));
        }

        [Fact]
        public async Task Throws_when_unsupported_method_requested()
        {
            var provider = new EmptyProvider();
            
            Assert.False(provider.Can(RequestMethod.Get));
            Assert.False(provider.Can(RequestMethod.Put));
            Assert.False(provider.Can(RequestMethod.Post));
            Assert.False(provider.Can(RequestMethod.Delete));
            await Assert.ThrowsAnyAsync<DynamicException>(async () => await provider.GetAsync("noop", ImmutableContainer.Empty));
        }

        [Fact]
        public async Task Throws_when_resource_format_not_specified()
        {
            var provider = new SimpleProvider();
            Assert.True(provider.Can(RequestMethod.Get));
            await provider.GetAsync("test:///noop", ImmutableContainer.Empty);
        }

        private class EmptyProvider : ResourceProvider
        {
            public EmptyProvider() : base(ImmutableContainer.Empty.SetScheme("test"))
            {
                Methods = MethodDictionary.Empty;
            }
        }

        private class SimpleProvider : ResourceProvider
        {
            public SimpleProvider() : base(ImmutableContainer.Empty.SetScheme("test"))
            {
                Methods =
                    MethodDictionary
                        .Empty
                        .Add(RequestMethod.Get, r => Task.FromResult<IResource>(new InMemoryResource(ImmutableContainer.Empty, Stream.Null)));
            }
        }
    }
}