using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using System.Linq.Expressions;

namespace Reusable.Reflection
{
    public interface IResourceReader
    {
        [NotNull, ItemNotNull]
        IEnumerable<string> GetResourceNames([NotNull] Assembly assembly);

        [NotNull]
        Stream GetStream([NotNull] Assembly assembly, [NotNull] string name);

        [NotNull]
        string GetString([NotNull] Assembly assembly, [NotNull] string name);
    }

    public class ResourceReader : IResourceReader
    {
        //[NotNull]
        //public static readonly IResourceRepository Default = new ResourceRepository();

        public IEnumerable<string> GetResourceNames(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            return assembly.GetManifestResourceNames();
        }

        public Stream GetStream(Assembly assembly, string name)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (name == null) throw new ArgumentNullException(nameof(name));
            return
                assembly.GetManifestResourceStream(name)
                    ?? throw DynamicException.Factory.CreateDynamicException(
                            $"ResourceNotFound{nameof(Exception)}",
                            $"Resource '{name}' not found in the '{assembly.GetName().Name}' assembly.",
                            null);
        }

        public string GetString(Assembly assembly, string name)
        {
            using (var resourceStream = GetStream(assembly, name))
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }

    public static class ResourceReader<T>
    {
        [NotNull]
        // ReSharper disable once StaticMemberInGenericType
        private static readonly IResourceReader Resources = new ResourceReader();

        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetResourceNames()
        {
            return Resources.GetResourceNames(typeof(T).Assembly);
        }

        [NotNull]
        public static Stream FindStream([NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            return Resources.FindStream<T>(predicateExpression);
        }

        [NotNull]
        public static string FindString([NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            return Resources.FindString<T>(predicateExpression);
        }

        [NotNull]
        public static string FindName([NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            return Resources.FindName<T>(predicateExpression);
        }
    }
}
