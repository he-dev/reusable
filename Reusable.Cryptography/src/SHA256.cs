using System;
using System.Security.Cryptography;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming - we want to keep the incosistent naming for SHA1

namespace Reusable.Cryptography
{
    public static class SHA256
    {
        [NotNull]
        public static byte[] ComputeSHA256([NotNull] this byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            using (var sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(source);
            }
        }
    }
}
