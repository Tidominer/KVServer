using Microsoft.EntityFrameworkCore;
using KVServer.Core.Models;
using KVServer.Core.Repositories;
using KVServer.Infrastructure.Data;

namespace KVServer.Infrastructure.Repositories;

public class VersionRepository : IVersionRepository
{
    private readonly KVServerDbContext _context;

    public VersionRepository(KVServerDbContext context)
    {
        _context = context;
    }

    public async Task<VersionEntry?> GetByIdAsync(int id)
    {
        return await _context.Versions
            .Include(v => v.Key)
            .ThenInclude(k => k.Storage)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<VersionEntry?> GetByKeyAndVersionAsync(int keyId, int versionNumber)
    {
        return await _context.Versions
            .Include(v => v.Key)
            .ThenInclude(k => k.Storage)
            .FirstOrDefaultAsync(v => v.KeyId == keyId && v.VersionNumber == versionNumber);
    }

    public async Task<IEnumerable<VersionEntry>> GetByKeyIdAsync(int keyId)
    {
        return await _context.Versions
            .Where(v => v.KeyId == keyId)
            .OrderBy(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<VersionEntry> CreateAsync(VersionEntry version)
    {
        _context.Versions.Add(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task UpdateManyAsync(IEnumerable<VersionEntry> versions)
    {
        _context.Versions.UpdateRange(versions);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextVersionNumberAsync(int keyId)
    {
        var lastVersion = await _context.Versions
            .Where(v => v.KeyId == keyId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync();

        return lastVersion?.VersionNumber + 1 ?? 1;
    }

    public async Task DeleteAsync(int id)
    {
        var version = await _context.Versions.FindAsync(id);
        if (version != null)
        {
            _context.Versions.Remove(version);
            await _context.SaveChangesAsync();
        }
    }
}