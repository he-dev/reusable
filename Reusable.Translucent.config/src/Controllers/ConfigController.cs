using System;
using Reusable.Data;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    public abstract class ConfigController : ResourceController
    {
        public static readonly string Scheme = "config";

        public static readonly string ResourceNameQueryKey = "name";
        
        protected ConfigController(IImmutableContainer properties)
            : base(properties.UpdateItem(Schemes, s => s.Add(Scheme))) { }

        protected string GetResourceName(UriString uriString)
        {
            // config:settings?name=ESCAPED
            return Uri.UnescapeDataString(uriString.Query[ResourceNameQueryKey].ToString());
        }

        // ReSharper disable once InconsistentNaming
        protected Response OK(Request request, string body, string actualName)
        {
            return new Response.OK
            {
                Body = body,
                Metadata =
                    ImmutableContainer
                        .Empty
                        .Union(request.Metadata.Copy<ResourceProperties>())
                        .SetItem(Converter, Properties.GetItem(Converter))
                        .SetItem(Response.ActualName, actualName)
            };
        }

        #region Properties

        private static readonly From<ConfigController> This;

        public static Selector<ITypeConverter> Converter { get; } = This.Select(() => Converter);

        #endregion
    }
}