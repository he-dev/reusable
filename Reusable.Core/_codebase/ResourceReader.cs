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
        public static IEnumerable<string> FindEmbeddedResources<TAssembly>(Func<string, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return
                GetEmbededResourceNames<TAssembly>()
                    .Where(predicate)
                    .Select(name => ReadEmbeddedResource(typeof(TAssembly), name))
                    .Where(Conditional.IsNotNullOrEmpty);
        }

        public static IEnumerable<string> GetEmbededResourceNames<TAssembly>()
        {
            var assembly = Assembly.GetAssembly(typeof(TAssembly));
            return assembly.GetManifestResourceNames();
        }

        //public static string ReadEmbeddedResource<TAssembly, TNamespace>(string name)
        //{
        //    if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        //    return ReadEmbeddedResource(typeof(TAssembly), typeof(TNamespace), name);
        //}

        public static string ReadEmbeddedResource<TAssembly>(string name)
        {
            return ReadEmbeddedResource(typeof(TAssembly), name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name)));
        }

        //public static string ReadEmbeddedResource(Type assemblyType, Type namespaceType, string name)
        //{
        //    if (assemblyType == null) throw new ArgumentNullException(nameof(assemblyType));
        //    if (namespaceType == null) throw new ArgumentNullException(nameof(namespaceType));
        //    if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        //    return ReadEmbeddedResource(assemblyType, $"{namespaceType.Namespace}.{name}");
        //}

        public static string ReadEmbeddedResource(Type assemblyType, string name)
        {
            if (assemblyType == null) throw new ArgumentNullException(nameof(assemblyType));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            var assembly = Assembly.GetAssembly(assemblyType);
            using (var resourceStream = assembly.GetManifestResourceStream(name))
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
