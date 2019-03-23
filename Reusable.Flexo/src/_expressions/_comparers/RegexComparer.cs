using System.Collections.Generic;
using System.Text.RegularExpressions;
using Reusable.Collections;

namespace Reusable.Flexo {
    public class RegexComparer : EqualityComparerProvider
    {
        public RegexComparer(string name) : base(name ?? nameof(RegexComparer)) { }

        public bool IgnoreCase { get; set; }

        protected override Constant<IEqualityComparer<object>> InvokeCore(IExpressionContext context)
        {
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            var comparer = EqualityComparerFactory<object>.Create
            (
                @equals: (left, right) => Regex.IsMatch((string)right, (string)left, options),
                getHashCode: (obj) => 0
            );
            return (Name, comparer, context);
        }
    }
}