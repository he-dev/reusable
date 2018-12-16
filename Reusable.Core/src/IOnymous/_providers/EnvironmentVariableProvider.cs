using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Flawless;

namespace Reusable.IOnymous
{
    public partial class EnvironmentVariableProvider : ResourceProvider
    {
        private static readonly IExpressValidator<SimpleUri> UriValidator = ExpressValidator.For<SimpleUri>(builder =>
        {
            builder.BlockNull();
            builder.Ensure(x => SoftString.Comparer.Equals((string)x.Scheme, "file"));
        });

        private readonly IResourceProvider _resourceProvider;

        public EnvironmentVariableProvider([NotNull] IResourceProvider resourceProvider) 
            : base(resourceProvider.Metadata)
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        }

        public override Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            UriValidator.Validate(uri).ThrowIWhenInvalid();
            return _resourceProvider.GetAsync(Environment.ExpandEnvironmentVariables(uri.Path), metadata);
        }

        public override Task<IResourceInfo> PutAsync(SimpleUri uri, Stream value, ResourceProviderMetadata metadata = null)
        {
            UriValidator.Validate(uri).ThrowIWhenInvalid();
            return _resourceProvider.PutAsync(Environment.ExpandEnvironmentVariables(uri.Path), value, metadata);
        }

        public override Task<IResourceInfo> PutAsync(SimpleUri uri, object value, ResourceProviderMetadata metadata = null)
        {
            UriValidator.Validate(uri).ThrowIWhenInvalid();
            return _resourceProvider.PutAsync(Environment.ExpandEnvironmentVariables(uri.Path), value, metadata);
        }

        public override Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            UriValidator.Validate(uri).ThrowIWhenInvalid();
            return _resourceProvider.DeleteAsync(Environment.ExpandEnvironmentVariables(uri.Path), metadata);
        }
    }
}