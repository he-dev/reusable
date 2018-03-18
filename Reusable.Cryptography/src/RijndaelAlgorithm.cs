using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Cryptography
{
    public interface ISymmetricAlgorithm
    {
        [NotNull]
        string Encrypt([NotNull] string plainText, [NotNull] string keyOrPassword);

        [NotNull]
        string Decrypt([NotNull] string encryptedText, [NotNull] string password);
    }

    public class RijndaelAlgorithm : ISymmetricAlgorithm
    {
        private const string Salt = "PsDWinS€rVIC€s";

        private const string RgbVi = "@1B2c3D4e5F6g7H8";

        public string Encrypt(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText)) throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            var textBytes = Encoding.UTF8.GetBytes(plainText);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(Salt)))
            using (var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros })
            using (var encryptor = symmetricKey.CreateEncryptor(deriveBytes.GetBytes(256 / 8), Encoding.ASCII.GetBytes(RgbVi)))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(textBytes, 0, textBytes.Length);
                cryptoStream.FlushFinalBlock();
                var cipherTextBytes = memoryStream.ToArray();
                return Convert.ToBase64String(cipherTextBytes);
            }
        }

        public string Decrypt(string encryptedText, string password)
        {
            if (string.IsNullOrEmpty(encryptedText)) throw new ArgumentNullException(nameof(encryptedText));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));


            var cipherTextBytes = Convert.FromBase64String(encryptedText);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(Salt)))
            using (var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (var decryptor = symmetricKey.CreateDecryptor(deriveBytes.GetBytes(256 / 8), Encoding.ASCII.GetBytes(RgbVi)))
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
            }
        }        
    }
}
