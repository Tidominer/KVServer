using Microsoft.EntityFrameworkCore;
using KVServer.Core.Models;
using KVServer.Core.Repositories;
using KVServer.Infrastructure.Data;

namespace KVServer.Infrastructure.Repositories;

public class KeyRepository : IKeyRepository
{
    private readonly KVServerDbContext _context;

    public KeyRepository(KVServerDbContext context)
    {
        _context = context;
    }

    public async Task<Key?> GetByIdAsync(int id)
    {
        return await _context.Keys
            .Include(k => k.Storage)
            .Include(k => k.Versions)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Key?> GetByStorageAndKeyNameAsync(int storageId, string keyName)
    {
        return await _context.Keys
            .Include(k => k.Storage)
            .Include(k => k.Versions)
            .FirstOrDefaultAsync(k => k.StorageId == storageId && k.KeyName == keyName);
    }

    public async Task<IEnumerable<Key>> GetByStorageIdAsync(int storageId)
    {
        return await _context.Keys
            .Include(k => k.Versions)
            .Where(k => k.StorageId == storageId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<Key> CreateAsync(Key key)
    {
        _context.Keys.Add(key);
        await _context.SaveChangesAsync();
        return key;
    }

    public async Task<Key> UpdateAsync(Key key)
    {
        _context.Keys.Update(key);
        await _context.SaveChangesAsync();
        return key;
    }

    public async Task DeleteAsync(int id)
    {
        var key = await _context.Keys.FindAsync(id);
        if (key != null)
        {
            _context.Keys.Remove(key);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int storageId, string keyName)
    {
        return await _context.Keys
            .AnyAsync(k => k.StorageId == storageId && k.KeyName == keyName);
    }
}