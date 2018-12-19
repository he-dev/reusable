namespace Reusable.IOnymous
{
    public static class SimpleUriExtensions
    {

        public static bool IsIOnymous(this UriString uri) => SoftString.Comparer.Equals(uri.Scheme, ResourceProvider.DefaultScheme);
    }
}