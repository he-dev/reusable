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
    [PublicAPI]
    public static class ResourceReader
    {
        [CanBeNull]
        [ContractAnnotation("name:null => halt")]
        public static string ReadEmbeddedResource([NotNull] string name, [NotNull] Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            using (var resourceStream = assembly.GetManifestResourceStream(name))
            {
                if (resourceStream == null) return null;
                using (var streamReader = new StreamReader(resourceStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        [CanBeNull]
        [ContractAnnotation("namespaceProvider:null => halt; name:null => halt")]
        public static string ReadEmbeddedResource([NotNull] string name, [NotNull] Type namespaceProvider)
        {
            if (namespaceProvider == null) throw new ArgumentNullException(nameof(namespaceProvider));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            return ReadEmbeddedResource($"{namespaceProvider.Namespace}.{name}", Assembly.GetAssembly(namespaceProvider));
        }


        [CanBeNull]
        [ContractAnnotation("name:null => halt")]
        public static string ReadEmbeddedResource<TNamespaceProvider>([NotNull] string name)
        {
            return ReadEmbeddedResource(name, typeof(TNamespaceProvider));
        }

        [CanBeNull]
        [ContractAnnotation("name:null => halt")]
        public static string ReadEmbeddedResource([NotNull] string name)
        {
            return ReadEmbeddedResource(name, Assembly.GetCallingAssembly());
        }

        [NotNull]
        [ItemNotNull]
        [ContractAnnotation("null => halt")]
        public static IEnumerable<string> FindEmbeddedResources<TNamespaceProvider>([NotNull] Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return
                GetEmbededResourceNames<TNamespaceProvider>()
                    .Where(predicate)
                    .Select(name => ReadEmbeddedResource(name, typeof(TNamespaceProvider)))
                    .Where(Conditional.IsNotNullOrEmpty);
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<string> GetEmbededResourceNames<TNamespaceProvider>()
        {
            var assembly = Assembly.GetAssembly(typeof(TNamespaceProvider));
            return assembly.GetManifestResourceNames();
        }
    }
}
