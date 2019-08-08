using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.IOnymous.Config
{
    public abstract class SettingProvider : ResourceProvider
    {
        protected SettingProvider(IImmutableContainer properties)
            : base(properties.SetScheme("config")) { }

        protected string GetResourceName(UriString uriString)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(uriString.Query[SettingRequestBuilder.NameKey].ToString());
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class SettingProperty : SelectorBuilder<SettingProperty>
    {
        public static Selector<ITypeConverter> Converter { get; } = Select(() => Converter);
    }
}