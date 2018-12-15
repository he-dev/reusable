using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    public partial class RelativeValueProvider : ValueProvider
    {
        private readonly IValueProvider _valueProvider;

        private readonly string _basePath;

        public RelativeValueProvider([NotNull] IValueProvider valueProvider, [NotNull] string basePath)
            : base(valueProvider.Metadata)
        {
            _valueProvider = valueProvider ?? throw new ArgumentNullException(nameof(valueProvider));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public override async Task<IValueInfo> GetValueInfoAsync(string path, ValueProviderMetadata metadata = null)
        {
            return await _valueProvider.GetValueInfoAsync(CreateFullPath(path));
        }

        public override Task<IValueInfo> SerializeAsync(string name, Stream value, ValueProviderMetadata metadata = null)
        {
            return _valueProvider.SerializeAsync(CreateFullPath(name), value, metadata);
        }

        public override Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            return _valueProvider.SerializeAsync(CreateFullPath(name), value, metadata);
        }

        public override Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            return _valueProvider.DeleteAsync(name, metadata);
        }

        private string CreateFullPath(string path) => Path.Combine(_basePath, path ?? throw new ArgumentNullException(nameof(path)));
    }
}