using KVServer.Core.Models;

namespace KVServer.Core.Services;

public interface IKeyService
{
    Task<Key> CreateKeyAsync(int storageId, string keyName, string value, string createdBy);
    Task<(Key Key, bool Created)> UpsertKeyAsync(int storageId, string keyName, string value, string createdBy);
    Task ReEncryptAllAsync(int storageId, string oldEncKey, string newEncKey);
    Task<Key?> UpdateKeyAsync(int storageId, string keyName, string value, string createdBy);
    Task<string?> GetKeyValueAsync(int storageId, string keyName, string token);
    Task<IEnumerable<Key>> GetKeysByStorageIdAsync(int storageId);
    Task DeleteKeyAsync(int storageId, string keyName);
    Task<VersionEntry?> GetKeyVersionAsync(int storageId, string keyName, int versionNumber);
    Task<IEnumerable<VersionEntry>> GetKeyHistoryAsync(int storageId, string keyName);
}