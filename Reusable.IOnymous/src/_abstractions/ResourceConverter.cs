using System;
using System.Collections.Immutable;

namespace Reusable.IOnymous
{
    public abstract class ResourceConverter : ResourceProvider
    {
        protected readonly IResourceProvider ResourceProvider;

        protected readonly IImmutableSet<Type> SupportedTypes;

        protected ResourceConverter(IResourceProvider resourceProvider, params Type[] supportedTypes) 
            : base(resourceProvider.Schemes, resourceProvider.Metadata)
        {
            ResourceProvider = resourceProvider;
            SupportedTypes = supportedTypes?.ToImmutableHashSet() ?? throw new ArgumentNullException(nameof(supportedTypes));
        }
    }
}