using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable
{
    public partial class SoftString 
    {
        public static explicit operator string(SoftString obj) => obj?._value;

        public static implicit operator SoftString(string value) => value == null ? default(SoftString) : new SoftString(value);

        public static bool operator ==(SoftString left, SoftString right) => Comparer.Equals(left, right);

        public static bool operator !=(SoftString left, SoftString right) => !(left == right);

        public static bool operator ==(SoftString left, string right) => Comparer.Equals(left, right);

        public static bool operator !=(SoftString left, string right) => !(left == right);

        public static bool operator ==(string left, SoftString right) => Comparer.Equals(left, right);

        public static bool operator !=(string left, SoftString right) => !(left == right);
    }
}
