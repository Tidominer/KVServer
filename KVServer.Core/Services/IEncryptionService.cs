namespace KVServer.Core.Services;

public interface IEncryptionService
{
    /// <summary>
    /// Derives a cryptographic key from the access token and salt using PBKDF2
    /// </summary>
    string DeriveKey(string token, string salt);

    /// <summary>
    /// Encrypts plaintext using AES-256-GCM
    /// </summary>
    (string EncryptedValue, string IV) Encrypt(string plaintext, string key);

    /// <summary>
    /// Decrypts ciphertext using AES-256-GCM
    /// </summary>
    string Decrypt(string encryptedValue, string iv, string key);

    /// <summary>
    /// Generates a random salt for key derivation
    /// </summary>
    string GenerateSalt();
}