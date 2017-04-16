namespace Reusable.ConfigWhiz
{
    public class SettingPathFormat
    {
        private SettingPathFormat(string format) => Format = format;
        public string Format { get; }
        public static readonly SettingPathFormat ShortWeak = new SettingPathFormat(".a");
        public static readonly SettingPathFormat ShortStrong = new SettingPathFormat(".a[]");
        public static readonly SettingPathFormat FullWeak = new SettingPathFormat("a.b");
        public static readonly SettingPathFormat FullStrong = new SettingPathFormat("a.b[]");
        public static implicit operator string(SettingPathFormat format) => format.Format;
    }
}