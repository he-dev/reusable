using System;
using System.Reflection;

namespace Reusable.IO
{
    public static class DecoratorExtensions
    {
        public static IFileProvider DecorateWith(this IFileProvider decorable, Func<IFileProvider, IFileProvider> createDecorator)
        {
            return createDecorator(decorable);
        }
    }

    public partial class RelativeFileProvider
    {
        public static Func<IFileProvider, RelativeFileProvider> Create(string basePath)
        {
            return decorable => new RelativeFileProvider(decorable, basePath);
        }
    }

    public partial class EnvironmentVariableFileProvider
    {
        public static Func<IFileProvider, EnvironmentVariableFileProvider> Create()
        {
            return decorable => new EnvironmentVariableFileProvider(decorable);
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
