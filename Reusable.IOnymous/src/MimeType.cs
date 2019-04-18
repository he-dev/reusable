using System;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public readonly struct MimeType : IEquatable<MimeType>
    {
        public MimeType(string name) => Name = name;

        [AutoEqualityProperty]
        public SoftString Name { get; }

        public bool IsNull => this == Null;

        public static readonly MimeType Null = new MimeType(string.Empty);

        /// <summary>
        /// Any document that contains text and is theoretically human readable
        /// </summary>
        public static readonly MimeType Text = new MimeType("text/plain");

        public static readonly MimeType Json = new MimeType("application/json");

        /// <summary>
        /// Any kind of binary data, especially data that will be executed or interpreted somehow.
        /// </summary>
        public static readonly MimeType Binary = new MimeType("application/octet-stream");

        public override bool Equals(object obj) => obj is MimeType format && Equals(format);

        public bool Equals(MimeType other) => AutoEquality<MimeType>.Comparer.Equals(this, other);

        public override int GetHashCode() => AutoEquality<MimeType>.Comparer.GetHashCode(this);

        public override string ToString() => Name.ToString();

        public static bool operator ==(MimeType left, MimeType right) => left.Equals(right);

        public static bool operator !=(MimeType left, MimeType right) => !(left == right);
    }
}