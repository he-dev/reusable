using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;

namespace Reusable
{
    public class ResourceReader
    {
        public static IEnumerable<string> FindEmbeddedResources<TNamespaceProvider>(Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return
                GetEmbededResourceNames<TNamespaceProvider>()
                    .Where(predicate)
                    .Select(name => ReadEmbeddedResource(typeof(TNamespaceProvider), name))
                    .Where(Conditional.IsNotNullOrEmpty);
        }

        public static IEnumerable<string> GetEmbededResourceNames<TNamespaceProvider>()
        {
            var assembly = Assembly.GetAssembly(typeof(TNamespaceProvider));
            return assembly.GetManifestResourceNames();
        }

        //public static string ReadEmbeddedResource<TAssembly, TNamespace>(string name)
        //{
        //    if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        //    return ReadEmbeddedResource(typeof(TAssembly), typeof(TNamespace), name);
        //}

        public static string ReadEmbeddedResource<TNamespaceProvider>(string name)
        {
            return ReadEmbeddedResource(typeof(TNamespaceProvider), name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name)));
        }

        //public static string ReadEmbeddedResource(Type assemblyType, Type namespaceType, string name)
        //{
        //    if (assemblyType == null) throw new ArgumentNullException(nameof(assemblyType));
        //    if (namespaceType == null) throw new ArgumentNullException(nameof(namespaceType));
        //    if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        //    return ReadEmbeddedResource(assemblyType, $"{namespaceType.Namespace}.{name}");
        //}

        public static string ReadEmbeddedResource(Type namespaceType, string name)
        {
            if (namespaceType == null) throw new ArgumentNullException(nameof(namespaceType));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            var assembly = Assembly.GetAssembly(namespaceType);
            using (var resourceStream = assembly.GetManifestResourceStream($"{namespaceType.Namespace}.{name}"))
            {
                if (resourceStream == null) return null;
                using (var streamReader = new StreamReader(resourceStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
