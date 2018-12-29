using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.IOnymous;
using Reusable.Reflection;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public class SettingProvider : ResourceProvider
    {
        private readonly IResourceProvider _resourceProvider;

        public SettingProvider(IResourceProvider resourceProvider)
            : base(resourceProvider.Schemes.Add("setting"), resourceProvider.Metadata)
        {
            if (!resourceProvider.Schemes.Intersect(new SoftString[] { "setting", DefaultScheme }).Any())
            {
                throw new ArgumentException($"{resourceProvider.GetType().ToPrettyString()} must support scheme 'setting' or '{DefaultScheme}'.");
            }
            _resourceProvider = resourceProvider;
        }

        public static Func<IResourceProvider, IResourceProvider> Factory() => decorable => new SettingProvider(decorable);

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.GetAsync(Translate(uri), Translate(uri, metadata));
        }

        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(Translate(uri), value, Translate(uri, metadata));
        }

        protected override Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(Translate(uri), Translate(uri, metadata));
        }

        // uri=
        // setting:name-space.type.member?instance=name&prefix=name&strength=name&prefixhandling=name
        protected UriString Translate(UriString uri)
        {
            var providerConvention =
                SettingMetadata
                    .AssemblyAttributes
                    .FirstOrDefault(x => x.Matches(this)) ?? SettingProviderAttribute.Default; // In case there are not assembly-level attributes.;

            var memberStrength = (SettingNameStrength)Enum.Parse(typeof(SettingNameStrength), uri.Query["strength"].ToString(), ignoreCase: true);
            var strength = new[] { memberStrength, providerConvention.Strength }.First(x => x > SettingNameStrength.Inherit);
            var path = uri.Path.Decoded.ToString().Split('.').Skip(2 - (int)strength).Join(".");

            var memberPrefixHandling = (PrefixHandling)Enum.Parse(typeof(PrefixHandling), uri.Query["prefixHandling"].ToString(), ignoreCase: true);
            var prefixHandling = new[] { memberPrefixHandling, providerConvention.PrefixHandling }.First(x => x > PrefixHandling.Inherit);

            var memberPrefix = uri.Query.TryGetValue("prefix", out var p) ? p : (SoftString)string.Empty;
            var prefixes = new[] { memberPrefix, (SoftString)providerConvention.Prefix };

            var query = (SoftString)new (SoftString Key, SoftString Value)[]
                {
                    ("prefix", prefixHandling == PrefixHandling.Enable ? prefixes.First(x => x) : (SoftString)string.Empty),
                    ("instance", uri.Query.TryGetValue("instance", out var instance) ? instance : (SoftString)string.Empty)
                }
                .Where(x => x.Value)
                .Select(x => $"{x.Key}={x.Value}")
                .Join("&");

            return $"setting:{path}{(query ? $"?{query.ToString()}" : string.Empty)}";
        }

        protected ResourceMetadata Translate(UriString uri, ResourceMetadata metadata)
        {
            metadata = metadata ?? ResourceMetadata.Empty;

            if (uri.Query.TryGetValue("providerCustomName", out var providerCustomName))
            {
                metadata = metadata.ProviderCustomName(providerCustomName);
            }

            if (uri.Query.TryGetValue("providerDefaultName", out var providerDefaultName))
            {
                metadata = metadata.ProviderDefaultName(providerDefaultName);
            }

            return metadata;
        }
    }
}