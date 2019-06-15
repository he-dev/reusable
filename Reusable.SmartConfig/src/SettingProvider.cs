using System.Collections.Immutable;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    public abstract class SettingProvider : ResourceProvider
    {
        public static readonly ITypeConverter<UriString, string> DefaultUriStringConverter = new UriStringToStringConverter();

        public static readonly IImmutableList<SoftString> DefaultSchemes = ImmutableList<SoftString>.Empty.Add("config");
        
        protected SettingProvider(IImmutableSession metadata)
            : base(DefaultSchemes, metadata) { }
    }
}