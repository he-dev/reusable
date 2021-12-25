namespace Reusable.Essentials.Extensions;

public static class CryptographyExtensions
{
    public static byte[] ComputeSHA1(this byte[] source)
    {
        using var algorithm = System.Security.Cryptography.SHA1.Create();
        return algorithm.ComputeHash(source);
    }
    
    public static byte[] ComputeSHA256(this byte[] source)
    {
        using var algorithm = System.Security.Cryptography.SHA256.Create();
        return algorithm.ComputeHash(source);
    }
}