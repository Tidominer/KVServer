using KVServer.Core.Models;

namespace KVServer.Core.Services;

public interface IStorageService
{
    Task<Storage> CreateStorageAsync(string name);
    Task<Storage> UpdateStorageAsync(Storage storage);
    Task<Storage?> GetStorageByTokenAsync(string token);
    Task<Storage?> GetStorageByIdAsync(int id);
    Task<Storage?> GetStorageByNameAsync(string name);
    Task<IEnumerable<Storage>> GetAllStoragesAsync();
    Task DeleteStorageAsync(int id);
    Task<bool> ValidateStorageAccessAsync(string token);
}