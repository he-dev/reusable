using System;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Cryptography.Extensions
{
    public static class StringExtensions
    {
        [NotNull]
        public static byte[] ToBytes([NotNull] this string source, Encoding encoding = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return (encoding ?? Encoding.UTF8).GetBytes(source);
        }
    }
}