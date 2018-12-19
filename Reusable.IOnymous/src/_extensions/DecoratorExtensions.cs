using System;

namespace Reusable.IOnymous
{
    public static class DecoratorExtensions
    {
        public static IResourceProvider DecorateWith(this IResourceProvider decorable, Func<IResourceProvider, IResourceProvider> createDecorator)
        {
            return createDecorator(decorable);
        }
    }

    public partial class RelativeResourceProvider
    {
        public static Func<IResourceProvider, RelativeResourceProvider> Create(UriString baseUri)
        {
            return decorable => new RelativeResourceProvider(decorable, baseUri);
        }
    }

    public partial class EnvironmentVariableProvider
    {
        public static Func<IResourceProvider, EnvironmentVariableProvider> Create()
        {
            return decorable => new EnvironmentVariableProvider(decorable);
        }
    }

    /*
     
    There are no decorators for:
        - PhysicalFileProvider
        - EmbeddedFileProvider
        - InMemoryFileProvider

    because they are the low level providers
     
     */

}
