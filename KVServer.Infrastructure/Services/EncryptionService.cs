using System.Security.Cryptography;
using System.Text;
using KVServer.Core.Services;

namespace KVServer.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private const int KeySize = 32; // 256 bits
    private const int SaltSize = 32; // 256 bits
    private const int Iterations = 100000;
    private const int GcmTagSize = 16; // 128 bits
    private const int GcmNonceSize = 12; // 96 bits

    public string DeriveKey(string token, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            token,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return Convert.ToBase64String(key);
    }

    public (string EncryptedValue, string IV) Encrypt(string plaintext, string key)
    {
        var keyBytes = Convert.FromBase64String(key);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[GcmNonceSize];
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[GcmTagSize];

        // Generate random nonce
        RandomNumberGenerator.Fill(nonce);

        // Encrypt using AES-256-GCM
        using var aes = new AesGcm(keyBytes, GcmTagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Combine ciphertext + tag for storage
        var combined = new byte[ciphertext.Length + tag.Length];
        Buffer.BlockCopy(ciphertext, 0, combined, 0, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, combined, ciphertext.Length, tag.Length);

        return (
            Convert.ToBase64String(combined),
            Convert.ToBase64String(nonce)
        );
    }

    public string Decrypt(string encryptedValue, string iv, string key)
    {
        var keyBytes = Convert.FromBase64String(key);
        var nonce = Convert.FromBase64String(iv);
        var combined = Convert.FromBase64String(encryptedValue);

        // Separate ciphertext and tag
        var ciphertext = new byte[combined.Length - GcmTagSize];
        var tag = new byte[GcmTagSize];
        Buffer.BlockCopy(combined, 0, ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(combined, ciphertext.Length, tag, 0, tag.Length);

        // Decrypt using AES-256-GCM
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(keyBytes, GcmTagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public string GenerateSalt()
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        return Convert.ToBase64String(salt);
    }
}