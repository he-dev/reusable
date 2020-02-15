using System;
using Reusable.OneTo1;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent.Controllers
{
    /// <summary>
    /// This is the base class for controllers handling the 'config' scheme.
    /// </summary>
    [Handles(typeof(ConfigRequest))]
    public abstract class ConfigController : ResourceController
    {
        protected ConfigController(ControllerName name) : base(name, default) { }

        public ITypeConverter Converter { get; set; } = TypeConverter.PassThru;

        // protected static string GetResourceName(string uriString)
        // {
        //     // config:settings?name=ESCAPED
        //     return Uri.UnescapeDataString(uriString.Query[ResourceNameQueryKey].ToString());
        // }
    }
}