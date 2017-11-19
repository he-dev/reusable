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
    public partial class SoftString : IComparable<SoftString>, IComparable<string>
    {
        public int CompareTo(SoftString other) => Comparer.Compare(this, other);

        public int CompareTo(string other) => CompareTo((SoftString)other);
    }
}
