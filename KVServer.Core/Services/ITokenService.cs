namespace KVServer.Core.Services;

public interface ITokenService
{
    string GenerateToken(int storageId);
    (bool IsValid, int? StorageId) ValidateToken(string token);
}