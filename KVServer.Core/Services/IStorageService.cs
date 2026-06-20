using KVServer.Core.Models;

namespace KVServer.Core.Services;

public interface IStorageService
{
    Task<Storage> CreateStorageAsync(string name);
    Task<Storage?> GetStorageByTokenAsync(string token);
    Task<Storage?> GetStorageByIdAsync(int id);
    Task<IEnumerable<Storage>> GetAllStoragesAsync();
    Task DeleteStorageAsync(int id);
    Task<bool> ValidateStorageAccessAsync(string token);
}