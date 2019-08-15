namespace Reusable.Translucent
{
    public static class UriStringExtensions
    {
        public static bool IsIOnymous(this UriString uri) => SoftString.Comparer.Equals(uri.Scheme, UriSchemes.Custom.IOnymous);

        /// <summary>
        /// Gets the Universal Naming Convention (UNC) path for Windows.
        /// </summary>
        public static string ToUnc(this UriString uri)
        {
            var path = uri.Path.Decoded.ToString();
            
            return
                uri.Authority
                    ? $"//{(string)uri.Authority}/{path.TrimStart('/')}"
                    : path;
        }
    }
}