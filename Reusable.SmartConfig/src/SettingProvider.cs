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

        protected static readonly IExpressValidator<SimpleUri> UriValidator = ExpressValidator.For<SimpleUri>(assert =>
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

        public override Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            return _resourceProvider.GetAsync(Translate(uri), Translate(uri, metadata));
        }

        public override Task<IResourceInfo> PutAsync(SimpleUri uri, Stream value, ResourceProviderMetadata metadata = null)
        {
            return _resourceProvider.PutAsync(Translate(uri), value, Translate(uri, metadata));
        }

        public override Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            return _resourceProvider.DeleteAsync(Translate(uri), Translate(uri, metadata));
        }

        protected SimpleUri Validate(SimpleUri uri) => UriValidator.Validate(uri).Assert();

        // uri=
        // setting:name-space.type.member?instance=name&prefix=name&strength=name&prefixhandling=name
        protected SimpleUri Translate(SimpleUri uri)
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

        protected ResourceProviderMetadata Translate(SimpleUri uri, ResourceProviderMetadata metadata)
        {
            metadata = metadata ?? ResourceProviderMetadata.Empty;

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


    public interface ISettingProvider : IEquatable<ISettingProvider>
    {
        [NotNull]
        [AutoEqualityProperty]
        SoftString Name { get; }

        [CanBeNull]
        ISetting Read([NotNull] SelectQuery query);

        void Write([NotNull] UpdateQuery query);
    }

    [PublicAPI]
    public abstract class SettingProvider : ISettingProvider
    {
        private readonly ISettingConverter _converter;

        private ISettingNameFactory _settingNameFactory;

        private SoftString _name;

        private SettingProvider()
        {
            Name = this.CreateDefaultName();
        }

        protected SettingProvider([NotNull] ISettingNameFactory settingNameFactory, [NotNull] ISettingConverter converter)
            : this()
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _settingNameFactory = settingNameFactory ?? throw new ArgumentNullException(nameof(settingNameFactory));
        }

        public SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public ISetting Read(SelectQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var providerNaming = this.SettingNaming(query);
            var providerSettingName = _settingNameFactory.CreateProviderSettingName(query.SettingName, providerNaming);

            try
            {
                var setting = Read(providerSettingName);

                return
                    setting is null
                        ? default
                        : new Setting
                        (
                            setting.Name,
                            setting.Value is null ? default : _converter.Deserialize(setting.Value, query.SettingType)
                        );
            }
            catch (Exception innerException)
            {
                throw ($"ReadSetting", $"An error occured while trying to read {providerSettingName.ToString().QuoteWith("'")} from {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        [CanBeNull]
        protected abstract ISetting Read(SettingName name);

        public void Write(UpdateQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var providerNaming = this.SettingNaming(query);
            var providerSettingName = _settingNameFactory.CreateProviderSettingName(query.SettingName, providerNaming);

            try
            {
                var value = query.Value is null ? null : _converter.Serialize(query.Value);
                Write(new Setting(providerSettingName, value));
            }
            catch (Exception innerException)
            {
                throw ("WriteSetting", $"An error occured while trying to write {providerSettingName.ToString().QuoteWith("'")} to {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract void Write(ISetting setting);

        #region IEquatable<ISettingProvider>

        public bool Equals(ISettingProvider other) => AutoEquality<ISettingProvider>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is ISettingProvider other && Equals(other);

        public override int GetHashCode() => AutoEquality<ISettingProvider>.Comparer.GetHashCode(this);

        #endregion
    }

    public class SettingProviderNaming
    {
        public SettingNameStrength Strength { get; set; }

        public string Prefix { get; set; }

        public PrefixHandling PrefixHandling { get; set; }
    }
}