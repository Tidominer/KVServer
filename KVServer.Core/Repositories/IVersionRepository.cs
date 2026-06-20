using KVServer.Core.Models;

namespace KVServer.Core.Repositories;

public interface IVersionRepository
{
    Task<VersionEntry?> GetByIdAsync(int id);
    Task<VersionEntry?> GetByKeyAndVersionAsync(int keyId, int versionNumber);
    Task<IEnumerable<VersionEntry>> GetByKeyIdAsync(int keyId);
    Task<VersionEntry> CreateAsync(VersionEntry version);
    Task<int> GetNextVersionNumberAsync(int keyId);
    Task DeleteAsync(int id);
}