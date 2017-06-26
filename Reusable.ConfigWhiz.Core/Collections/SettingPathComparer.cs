using System;
using System.Collections.Generic;
using Reusable.ConfigWhiz.Paths;

namespace Reusable.ConfigWhiz.Collections
{
    public class SettingPathComparer : IEqualityComparer<Identifier>
    {
        private static readonly SettingPathFormatter Formatter = new SettingPathFormatter();

        public SettingPathComparer(SettingPathFormat format)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
        }

        public SettingPathFormat Format { get; }

        public bool Equals(Identifier x, Identifier y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.ToString(Format, Formatter).Equals(y.ToString(Format, Formatter), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Identifier obj)
        {
            return obj.ToString(Format, Formatter).GetHashCode();
        }
    }
}