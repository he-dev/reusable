using System;
using Reusable.OneTo1;

namespace Reusable.Translucent.Controllers
{
    /// <summary>
    /// This is the base class for controllers handling the 'config' scheme.
    /// </summary>
    [Handles(typeof(ConfigRequest))]
    public abstract class ConfigController : ResourceController
    {
        public static readonly string Scheme = "config";

        public static readonly string ResourceNameQueryKey = "name";

        protected ConfigController(ControllerName controllerName) : base(controllerName, default) { }

        public ITypeConverter Converter { get; set; } = TypeConverter.PassThru;

        protected static string GetResourceName(UriString uriString)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(uriString.Query[ResourceNameQueryKey].ToString());
        }
    }
}