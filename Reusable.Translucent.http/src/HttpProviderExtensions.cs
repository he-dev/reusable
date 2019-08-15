using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public static class HttpProviderExtensions
    {
        #region GET helpers

        public static Task<IResource> GetHttpAsync(this IResourceProvider resourceProvider, string path, IImmutableContainer properties = default)
        {
            var uri = new UriString(path);
            uri =
                uri.IsAbsolute
                    ? uri
                    : new UriString(UriSchemes.Known.Http, (string)uri.Path.Original);

            return resourceProvider.GetAsync(uri, r => r.Properties = r.Properties.Union(properties.ThisOrEmpty()));
        }

        #endregion

        #region PUT helpers

        #endregion

        #region POST helpers

        #endregion

        #region DELETE helpers

        #endregion
    }
}