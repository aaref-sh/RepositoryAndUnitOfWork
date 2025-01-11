using System.Security.Cryptography;

namespace Helper.Helpers;

public class AES
{
    private static readonly int SaltSize = 32; // 32 bytes for the salt
    private static readonly int KeySize = 256; // 256 bits for the key
    private static readonly int Iterations = 1000; // Number of iterations for the key derivation

    public static string Encrypt(string plainText, string password)
    {
        // Generate a random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt);
        }

        // Derive the key and IV from the password and salt
        var key = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] keyBytes = key.GetBytes(KeySize / 8);
        byte[] ivBytes = key.GetBytes(16); // 16 bytes for the IV

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        // Write the salt to the beginning of the memory stream
        ms.Write(salt, 0, salt.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText, string password)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        // Extract the salt from the beginning of the cipher bytes
        byte[] salt = new byte[SaltSize];
        Array.Copy(cipherBytes, 0, salt, 0, salt.Length);

        // Derive the key and IV from the password and salt
        var key = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] keyBytes = key.GetBytes(KeySize / 8);
        byte[] ivBytes = key.GetBytes(16); // 16 bytes for the IV

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherBytes, salt.Length, cipherBytes.Length - salt.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
