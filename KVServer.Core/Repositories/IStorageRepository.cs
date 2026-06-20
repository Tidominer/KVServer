using KVServer.Core.Models;

namespace KVServer.Core.Repositories;

public interface IStorageRepository
{
    Task<Storage?> GetByIdAsync(int id);
    Task<Storage?> GetByTokenAsync(string token);
    Task<Storage?> GetByNameAsync(string name);
    Task<IEnumerable<Storage>> GetAllAsync();
    Task<Storage> CreateAsync(Storage storage);
    Task<Storage> UpdateAsync(Storage storage);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}