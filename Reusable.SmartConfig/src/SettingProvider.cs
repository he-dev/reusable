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
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public class SettingProvider2 : ResourceProvider
    {
        private readonly IResourceProvider _resourceProvider;

        protected static readonly IExpressValidator<UriString> UriValidator = ExpressValidator.For<UriString>(assert =>
        {
            assert.NotNull();
            assert.True(x => x.IsAbsolute);
            assert.True(x => x.Scheme == "setting");
        });

        public SettingProvider2(IResourceProvider resourceProvider)
            : base(resourceProvider.Metadata)
        {
            _resourceProvider = resourceProvider;
        }

        public static Func<IResourceProvider, IResourceProvider> Factory() => decorable => new SettingProvider2(decorable);

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.GetAsync(Translate(uri), Translate(uri, metadata));
        }

        public override Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(Translate(uri), value, Translate(uri, metadata));
        }

        public override Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(Translate(uri), Translate(uri, metadata));
        }

        protected UriString Validate(UriString uri) => UriValidator.Validate(uri).Assert();

        // uri=
        // setting:name-space.type.member?instance=name&prefix=name&strength=name&prefixhandling=name
        protected UriString Translate(UriString uri)
        {
            Validate(uri);

            var providerConvention =
                SettingMetadata
                    .AssemblyAttributes
                    .FirstOrDefault(x => x.Matches(this)) ?? SettingProviderAttribute.Default; // In case there are not assembly-level attributes.;

            var memberStrength = (SettingNameStrength)Enum.Parse(typeof(SettingNameStrength), uri.Query["strength"], ignoreCase: true);
            var strength = new[] { memberStrength, providerConvention.Strength }.First(x => x > SettingNameStrength.Inherit);
            var path = uri.Path.Value.Split('.').Skip(2 - (int)strength).Join(".");

            var memberPrefixHandling = (PrefixHandling)Enum.Parse(typeof(PrefixHandling), uri.Query["prefixHandling"], ignoreCase: true);
            var prefixHandling = new[] { memberPrefixHandling, providerConvention.PrefixHandling }.First(x => x > PrefixHandling.Inherit);

            var query = (ImplicitString)new (ImplicitString Key, ImplicitString Value)[]
            {
                ("prefix", prefixHandling == PrefixHandling.Enable ? new [] { (uri.Query.TryGetValue("prefix", out var p) ? p : (ImplicitString)string.Empty), (ImplicitString)providerConvention.Prefix }.First(prefix => prefix) : (ImplicitString)string.Empty),
                ("instance", uri.Query.TryGetValue("instance", out var instance) ? instance :  (ImplicitString)string.Empty)
            }
            .Where(x => x.Value)
            .Select(x => $"{x.Key}={x.Value}")
            .Join("&");

            return $"setting:{path}{(query ? $"?{query}" : string.Empty)}";
        }

        protected ResourceMetadata Translate(UriString uri, ResourceMetadata metadata)
        {
            metadata = metadata ?? ResourceMetadata.Empty;

            if (uri.Query.TryGetValue("providerCustomName", out var providerCustomName))
            {
                metadata = metadata.Add("providerCustomName", (string)providerCustomName);
            }

            if (uri.Query.TryGetValue("providerDefaultName", out var providerDefaultName))
            {
                metadata = metadata.Add("providerDefaultName", (string)providerDefaultName);
            }

            return metadata;
        }
    }
}