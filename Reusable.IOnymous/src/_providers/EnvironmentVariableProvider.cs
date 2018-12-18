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
        private static readonly IExpressValidator<UriString> UriValidator = ExpressValidator.For<UriString>(builder =>
        {
            builder.NotNull();
            builder.True(x => SoftString.Comparer.Equals((string)x.Scheme, "file"));
        });

        private readonly IResourceProvider _resourceProvider;

        public EnvironmentVariableProvider([NotNull] IResourceProvider resourceProvider) 
            : base(resourceProvider.Metadata)
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            UriValidator.Validate(uri).Assert();
            return _resourceProvider.GetAsync(Environment.ExpandEnvironmentVariables(uri.Path), metadata);
        }

        public override Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            UriValidator.Validate(uri).Assert();
            return _resourceProvider.PutAsync(Environment.ExpandEnvironmentVariables(uri.Path), value, metadata);
        }

        public override Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            UriValidator.Validate(uri).Assert();
            return _resourceProvider.DeleteAsync(Environment.ExpandEnvironmentVariables(uri.Path), metadata);
        }
    }
}