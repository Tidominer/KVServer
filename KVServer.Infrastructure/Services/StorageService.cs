using KVServer.Core.Models;
using KVServer.Core.Repositories;
using KVServer.Core.Services;

namespace KVServer.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly IStorageRepository _storageRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ITokenService _tokenService;

    public StorageService(
        IStorageRepository storageRepository,
        IEncryptionService encryptionService,
        ITokenService tokenService)
    {
        _storageRepository = storageRepository;
        _encryptionService = encryptionService;
        _tokenService = tokenService;
    }

    public async Task<Storage> CreateStorageAsync(string name)
    {
        // Check if storage with same name already exists
        var existing = await _storageRepository.GetByNameAsync(name);
        if (existing != null)
            throw new InvalidOperationException($"Storage with name '{name}' already exists");

        // Create temporary storage to generate token
        var tempStorage = new Storage
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        // Generate token and salt
        var token = _tokenService.GenerateToken(0); // Temporary ID
        var salt = _encryptionService.GenerateSalt();

        tempStorage.AccessToken = token;
        tempStorage.Salt = salt;

        // Create storage in database
        var storage = await _storageRepository.CreateAsync(tempStorage);

        // Update token with actual storage ID
        var finalToken = _tokenService.GenerateToken(storage.Id);
        storage.AccessToken = finalToken;
        await _storageRepository.UpdateAsync(storage);

        return storage;
    }

    public async Task<Storage?> GetStorageByTokenAsync(string token)
    {
        return await _storageRepository.GetByTokenAsync(token);
    }

    public async Task<Storage?> GetStorageByIdAsync(int id)
    {
        return await _storageRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Storage>> GetAllStoragesAsync()
    {
        return await _storageRepository.GetAllAsync();
    }

    public async Task DeleteStorageAsync(int id)
    {
        await _storageRepository.DeleteAsync(id);
    }

    public async Task<bool> ValidateStorageAccessAsync(string token)
    {
        var storage = await GetStorageByTokenAsync(token);
        return storage != null;
    }
}