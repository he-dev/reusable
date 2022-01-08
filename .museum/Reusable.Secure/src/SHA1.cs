using System;
using System.Security.Cryptography;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming - we want to keep the inconsistent naming for SHA1

namespace Reusable.Cryptography
{
    public static class SHA1
    {
        [NotNull]
        public static byte[] ComputeHash([NotNull] byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            using (var sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(source);
            }
        }
    }
}
