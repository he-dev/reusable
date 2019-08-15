using System;
using Reusable.Data;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    public abstract class SettingController : ResourceController
    {
        protected SettingController(IImmutableContainer properties)
            : base(properties.UpdateItem(Schemes, s => s.Add("config"))) { }

        protected string GetResourceName(UriString uriString)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(uriString.Query[SettingRequestBuilder.NameKey].ToString());
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class SettingControllerProperties : SelectorBuilder<SettingControllerProperties>
    {
        public static Selector<ITypeConverter> Converter { get; } = Select(() => Converter);
    }
}