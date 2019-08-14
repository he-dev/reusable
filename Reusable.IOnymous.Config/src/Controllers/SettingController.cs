using System;
using Reusable.Data;
using Reusable.IOnymous.Config;
using Reusable.OneTo1;
using Reusable.Quickey;

// ReSharper disable once CheckNamespace
namespace Reusable.IOnymous.Controllers
{
    public abstract class SettingController : ResourceController
    {
        protected SettingController(IImmutableContainer properties)
            : base(properties.UpdateItem(ResourceControllerProperties.Schemes, s => s.Add("config"))) { }

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