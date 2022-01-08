using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Cryptography.Extensions
{
    public static class ByteExtensions
    {
        [NotNull]
        public static string ToHexString([NotNull] this byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return
                source
                    .Aggregate(new StringBuilder(), (current, next) => current.Append(next.ToString("X2")))
                    .ToString();
        }

        // This cannot be ToString because the compiler picks the wrong method otherwise.
        [NotNull] 
        public static string GetString([NotNull] this byte[] source, Encoding encoding = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return (encoding ?? Encoding.UTF8).GetString(source);
        }

        [ContractAnnotation("source: null => null; source: notnull => notnull")]
        public static string ToBase64String([NotNull] this byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return Convert.ToBase64String(source);
        }
    }
}
