using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    public partial class EnvironmentVariableProvider : ValueProvider
    {
        private readonly IValueProvider _valueProvider;

        public EnvironmentVariableProvider([NotNull] IValueProvider valueProvider) 
            : base(valueProvider.Metadata)
        {
            _valueProvider = valueProvider ?? throw new ArgumentNullException(nameof(valueProvider));
        }

        public override Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null)
        {
            return _valueProvider.GetValueInfoAsync(Environment.ExpandEnvironmentVariables(name), metadata);
        }

        public override Task<IValueInfo> SerializeAsync(string name, Stream value, ValueProviderMetadata metadata = null)
        {
            return _valueProvider.SerializeAsync(Environment.ExpandEnvironmentVariables(name), value, metadata);
        }

        public override Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            return _valueProvider.SerializeAsync(Environment.ExpandEnvironmentVariables(name), value, metadata);
        }

        public override Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            return _valueProvider.DeleteAsync(Environment.ExpandEnvironmentVariables(name), metadata);
        }
    }
}