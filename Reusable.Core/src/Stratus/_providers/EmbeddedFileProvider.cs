using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    using static ValueProviderMetadataKeyNames;

    public class EmbeddedFileProvider : ValueProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider([NotNull] Assembly assembly, ValueProviderMetadata metadata = null)
            : base(
                (metadata ?? ValueProviderMetadata.Empty)
                    .Add(CanDeserialize, true)
            )
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            BasePath = _assembly.GetName().Name.Replace('.', Path.DirectorySeparatorChar);
        }

        public string BasePath { get; }

        #region IFileProvider

        public override Task<IValueInfo> GetValueInfoAsync(string path, ValueProviderMetadata metadata = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Embedded resouce names are separated by '.' so replace the windows separator.
            var fullName = Path.Combine(BasePath, path).Replace(Path.DirectorySeparatorChar, '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));
            var getManifestResourceStream = actualName is null ? default(Func<Stream>) : () => _assembly.GetManifestResourceStream(actualName);

            return Task.FromResult<IValueInfo>(new EmbeddedFileInfo(UndoConvertPath(fullName), getManifestResourceStream));
        }

        public override Task<IValueInfo> SerializeAsync(string path, Stream data, ValueProviderMetadata metadata = null)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support value serialization.");
        }

        public override Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support value serialization.");
        }

        public override Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support value deletion.");
        }

        #endregion

        // Convert path back to windows format but the last '.' - this is the file extension.
        private static string UndoConvertPath(string path) => Regex.Replace(path, @"\.(?=.*?\.)", Path.DirectorySeparatorChar.ToString());
    }

    public static class EmbeddedFileProvider<T>
    {
        public static IValueProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly);
    }
}