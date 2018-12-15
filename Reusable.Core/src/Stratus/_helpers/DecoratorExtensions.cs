using System;

namespace Reusable.Stratus
{
    public static class DecoratorExtensions
    {
        public static IValueProvider DecorateWith(this IValueProvider decorable, Func<IValueProvider, IValueProvider> createDecorator)
        {
            return createDecorator(decorable);
        }
    }

    public partial class RelativeValueProvider
    {
        public static Func<IValueProvider, RelativeValueProvider> Create(string basePath)
        {
            return decorable => new RelativeValueProvider(decorable, basePath);
        }
    }

    public partial class EnvironmentVariableProvider
    {
        public static Func<IValueProvider, EnvironmentVariableProvider> Create()
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
