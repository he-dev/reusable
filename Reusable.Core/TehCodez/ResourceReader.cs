using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable
{
    public static class ResourceReader
    {
        [NotNull]
        [ItemNotNull]
        [ContractAnnotation("null => halt")]
        public static IEnumerable<string> FindEmbeddedResources<TNamespaceProvider>([NotNull] Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return
                GetEmbededResourceNames<TNamespaceProvider>()
                    .Where(predicate)
                    .Select(name => ReadEmbeddedResource(typeof(TNamespaceProvider), name))
                    .Where(Conditional.IsNotNullOrEmpty);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<string> GetEmbededResourceNames<TNamespaceProvider>()
        {
            var assembly = Assembly.GetAssembly(typeof(TNamespaceProvider));
            return assembly.GetManifestResourceNames();
        }

        [CanBeNull]
        [ContractAnnotation("name:null => halt")]
        public static string ReadEmbeddedResource<TNamespaceProvider>([NotNull] string name)
        {
            return ReadEmbeddedResource(typeof(TNamespaceProvider), name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name)));
        }

        [CanBeNull]
        [ContractAnnotation("namespaceProvider:null => halt; name:null => halt")]
        public static string ReadEmbeddedResource([NotNull] Type namespaceProvider, [NotNull] string name)
        {
            if (namespaceProvider == null) throw new ArgumentNullException(nameof(namespaceProvider));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            var assembly = Assembly.GetAssembly(namespaceProvider);
            using (var resourceStream = assembly.GetManifestResourceStream($"{namespaceProvider.Namespace}.{name}"))
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
