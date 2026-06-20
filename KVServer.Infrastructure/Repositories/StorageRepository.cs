using Microsoft.EntityFrameworkCore;
using KVServer.Core.Models;
using KVServer.Core.Repositories;
using KVServer.Infrastructure.Data;

namespace KVServer.Infrastructure.Repositories;

public class StorageRepository : IStorageRepository
{
    private readonly KVServerDbContext _context;

    public StorageRepository(KVServerDbContext context)
    {
        _context = context;
    }

    public async Task<Storage?> GetByIdAsync(int id)
    {
        return await _context.Storages
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    }

    public async Task<Storage?> GetByTokenAsync(string token)
    {
        return await _context.Storages
            .FirstOrDefaultAsync(s => s.AccessToken == token && s.IsActive);
    }

    public async Task<Storage?> GetByNameAsync(string name)
    {
        return await _context.Storages
            .FirstOrDefaultAsync(s => s.Name == name && s.IsActive);
    }

    public async Task<IEnumerable<Storage>> GetAllAsync()
    {
        return await _context.Storages
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Storage> CreateAsync(Storage storage)
    {
        _context.Storages.Add(storage);
        await _context.SaveChangesAsync();
        return storage;
    }

    public async Task<Storage> UpdateAsync(Storage storage)
    {
        _context.Storages.Update(storage);
        await _context.SaveChangesAsync();
        return storage;
    }

    public async Task DeleteAsync(int id)
    {
        var storage = await GetByIdAsync(id);
        if (storage != null)
        {
            storage.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Storages
            .AnyAsync(s => s.Id == id && s.IsActive);
    }
}