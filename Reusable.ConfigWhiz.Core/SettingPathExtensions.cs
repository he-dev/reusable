namespace Reusable.ConfigWhiz
{
    public static class SettingPathExtensions
    {
        //private static readonly SettingPathComparer FullWeakSettingPathComparer = new SettingPathComparer(SettingPathFormat.FullWeak);

        //public static bool IsLike(this SettingPath x, SettingPath y)
        //{
        //    return FullWeakSettingPathComparer.Equals(x, y);
        //}

        public static string ToShortWeakString(this SettingPath settingPath) => settingPath.ToString(SettingPathFormat.ShortWeak, SettingPathFormatter.Instance);
        public static string ToShortStrongString(this SettingPath settingPath) => settingPath.ToString(SettingPathFormat.ShortStrong, SettingPathFormatter.Instance);
        public static string ToFullWeakString(this SettingPath settingPath) => settingPath.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance);
        public static string ToFullStrongString(this SettingPath settingPath) => settingPath.ToString(SettingPathFormat.FullStrong, SettingPathFormatter.Instance);
    }
}