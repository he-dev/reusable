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
        /// <summary>
        /// Gets resource names from the specified assembly.
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable<string> GetResourceNames([NotNull] Assembly assembly);

        /// <summary>
        /// Gets resource stream with the exact name from the specified assembly.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        Stream GetStream([NotNull] string name, [NotNull] Assembly assembly);

        /// <summary>
        /// Gets resource string with the exact name from the specified assembly.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        [NotNull]
        string GetString([NotNull] string name, [NotNull] Assembly assembly);
    }

    public class ResourceReader : IResourceReader
    {
        [NotNull]
        public static readonly IResourceReader Default = new ResourceReader();

        public IEnumerable<string> GetResourceNames(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            return assembly.GetManifestResourceNames();
        }

        public Stream GetStream(string name, Assembly assembly)
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

        public string GetString(string name, Assembly assembly)
        {
            using (var resourceStream = GetStream(name, assembly))
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }   
}
