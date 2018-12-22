using System.Threading.Tasks;

namespace Reusable.IOnymous
{
    public static class ResourceInfoExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this IResourceInfo resourceInfo) => (T)(await resourceInfo.DeserializeAsync(typeof(T)));
        
        public static T Deserialize<T>(this IResourceInfo resourceInfo) => (T)(resourceInfo.DeserializeAsync(typeof(T)).GetAwaiter().GetResult());
    }
}