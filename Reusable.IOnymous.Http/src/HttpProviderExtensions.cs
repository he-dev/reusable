using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public static class HttpProviderExtensions
    {
        #region GET helpers

        public static Task<IResource> GetHttpAsync(this IResourceProvider resourceProvider, string path, ImmutableSession metadata = default)
        {
            var uri = new UriString(path);
            return resourceProvider.GetAsync
            (
                uri.IsAbsolute
                    ? uri
                    : new UriString(HttpProvider.DefaultScheme, (string)uri.Path.Original),
                metadata
            );
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