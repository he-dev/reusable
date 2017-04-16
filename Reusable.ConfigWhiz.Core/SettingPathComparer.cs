using System;
using System.Collections.Generic;

namespace Reusable.ConfigWhiz
{
    public class SettingPathComparer : IEqualityComparer<SettingPath>
    {
        private static readonly SettingPathFormatter Formatter = new SettingPathFormatter();

        public SettingPathComparer(SettingPathFormat format)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
        }

        public SettingPathFormat Format { get; }

        public bool Equals(SettingPath x, SettingPath y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.ToString(Format, Formatter).Equals(y.ToString(Format, Formatter), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(SettingPath obj)
        {
            return obj.ToString(Format, Formatter).GetHashCode();
        }
    }
}