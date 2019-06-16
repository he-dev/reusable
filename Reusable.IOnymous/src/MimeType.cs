using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public class MimeType : Option<MimeType>
    {
        public MimeType(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly MimeType Empty = CreateWithCallerName();

        /// <summary>
        /// Any document that contains text and is theoretically human readable
        /// </summary>
        public static readonly MimeType Text = CreateWithCallerName("text/plain");

        public static readonly MimeType Json = CreateWithCallerName("application/json");

        /// <summary>
        /// Any kind of binary data, especially data that will be executed or interpreted somehow.
        /// </summary>
        public static readonly MimeType Binary = CreateWithCallerName("application/octet-stream");
    }
}