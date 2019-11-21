using System;
using Reusable.Data;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    // Uses "Config" as name because controllers are named after their scheme. 
    public abstract class ConfigController : ResourceController
    {
        public static readonly string Scheme = "config";

        public static readonly string ResourceNameQueryKey = "name";

        protected ConfigController(string id) : base(id, Scheme) { }

        public ITypeConverter Converter { get; set; } = TypeConverter.PassThru;

        protected static string GetResourceName(UriString uriString)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(uriString.Query[ResourceNameQueryKey].ToString());
        }

        // ReSharper disable once InconsistentNaming
        protected Response OK(Request request, object body, string actualName)
        {
            return new Response
            {
                StatusCode = ResourceStatusCode.OK,
                Body = body
            };
        }
    }
}