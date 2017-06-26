namespace Reusable.ConfigWhiz.Paths
{
    public static class IdentifierExtensions
    {
        //private static readonly SettingPathComparer FullWeakSettingPathComparer = new SettingPathComparer(IdentifierFormat.FullWeak);

        //public static bool IsLike(this SettingPath x, SettingPath y)
        //{
        //    return FullWeakSettingPathComparer.Equals(x, y);
        //}

        public static string ToShortWeakString(this Identifier identifier) => identifier.ToString(IdentifierFormat.ShortWeak, IdentifierFormatter.Instance);
        public static string ToShortStrongString(this Identifier identifier) => identifier.ToString(IdentifierFormat.ShortStrong, IdentifierFormatter.Instance);
        public static string ToFullWeakString(this Identifier identifier) => identifier.ToString(IdentifierFormat.FullWeak, IdentifierFormatter.Instance);
        public static string ToFullStrongString(this Identifier identifier) => identifier.ToString(IdentifierFormat.FullStrong, IdentifierFormatter.Instance);
    }
}