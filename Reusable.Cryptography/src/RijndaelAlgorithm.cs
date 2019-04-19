using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Cryptography
{
    public interface ISymmetricAlgorithm
    {
        [NotNull]
        Task<byte[]> EncryptAsync([NotNull] byte[] data, [NotNull] string password);

        [NotNull]
        Task<byte[]> DecryptAsync([NotNull] byte[] data, [NotNull] string password);
    }

    public class RijndaelAlgorithm : ISymmetricAlgorithm
    {
        private readonly byte[] _salt;

        private readonly byte[] _rgbIV;

        public RijndaelAlgorithm(byte[] salt, byte[] rgbIV)
        {
            _salt = salt;
            _rgbIV = rgbIV;
        }

        public static byte[] CreateRgbIV()
        {
            using (var rnd = RandomNumberGenerator.Create())
            {
                var rgbIV = new byte[16];
                rnd.GetNonZeroBytes(rgbIV);
                return rgbIV;
            }
        }

        public async Task<byte[]> EncryptAsync(byte[] data, string password)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            using (var deriveBytes = new Rfc2898DeriveBytes(password, _salt))
            using (var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros })
            using (var encryptor = symmetricKey.CreateEncryptor(deriveBytes.GetBytes(256 / 8), _rgbIV))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                await cryptoStream.WriteAsync(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        public async Task<byte[]> DecryptAsync(byte[] data, string password)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            using (var deriveBytes = new Rfc2898DeriveBytes(password, _salt))
            using (var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (var decryptor = symmetricKey.CreateDecryptor(deriveBytes.GetBytes(256 / 8), _rgbIV))
            using (var memoryStream = new MemoryStream(data))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                var decrypted = new byte[data.Length];
                var decryptedByteCount = await cryptoStream.ReadAsync(decrypted, 0, decrypted.Length);
                return decrypted.TakeWhile(c => c != '\0').ToArray();
            }
        }
    }
}
