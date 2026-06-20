using KVServer.Core.Models;

namespace KVServer.Core.Repositories;

public interface IKeyRepository
{
    Task<Key?> GetByIdAsync(int id);
    Task<Key?> GetByStorageAndKeyNameAsync(int storageId, string keyName);
    Task<IEnumerable<Key>> GetByStorageIdAsync(int storageId);
    Task<Key> CreateAsync(Key key);
    Task<Key> UpdateAsync(Key key);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int storageId, string keyName);
}