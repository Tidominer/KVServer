using System.Security.Cryptography;
using KVServer.Core.Services;

namespace KVServer.Infrastructure.Services;

public class TokenService : ITokenService
{
    private const string TokenPrefix = "kv_";
    private const int SignatureLength = 16;

    public string GenerateToken(int storageId)
    {
        var signatureBytes = new byte[SignatureLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(signatureBytes);
        var signature = Convert.ToHexString(signatureBytes).ToLowerInvariant();
        return $"{TokenPrefix}{storageId}_{signature}";
    }

    public (bool IsValid, int? StorageId) ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token) || !token.StartsWith(TokenPrefix))
            return (false, null);

        var parts = token.Substring(TokenPrefix.Length).Split('_');
        if (parts.Length != 2) return (false, null);

        if (!int.TryParse(parts[0], out var storageId))
            return (false, null);

        return (true, storageId);
    }
}