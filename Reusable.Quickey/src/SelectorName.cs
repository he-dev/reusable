using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Quickey
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class SelectorName
    {
        public SelectorName(string value) => Value = value;

        private string DebuggerDisplay => ToString();

        public Type Type { get; set; }

        public string Prefix { get; set; }

        public string Value { get; }

        public string Suffix { get; set; }

        public override string ToString() => $"{Prefix}{Value}{Suffix}";

        [NotNull]
        public static implicit operator string(SelectorName selectorName) => selectorName.Value;

        [NotNull]
        public static implicit operator SoftString(SelectorName selectorName) => (string)selectorName;
    }

    public class SelectorIndex : SelectorName
    {
        public SelectorIndex(string value) : base(value) { }
    }
}