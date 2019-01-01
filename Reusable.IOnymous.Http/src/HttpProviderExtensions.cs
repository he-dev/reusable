using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.IOnymous
{
    public static class HttpProviderExtensions
    {
        #region GET helpers

        public static Task<IResourceInfo> GetHttpAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var uri = new UriString(path);
            return resourceProvider.GetAsync
            (
                uri.IsAbsolute
                    ? uri
                    : new UriString(HttpProvider.DefaultScheme,
                        (string)uri.Path.Original),
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