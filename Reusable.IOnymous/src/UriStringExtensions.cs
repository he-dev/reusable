namespace Reusable.IOnymous
{
    public static class UriStringExtensions
    {

        public static bool IsIOnymous(this UriString uri) => SoftString.Comparer.Equals(uri.Scheme, ResourceProvider.DefaultScheme);
    }
}