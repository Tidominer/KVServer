using KVServer.Core.Models;
using KVServer.Core.Repositories;
using KVServer.Core.Services;

namespace KVServer.Infrastructure.Services;

public class KeyService : IKeyService
{
    private readonly IKeyRepository _keyRepository;
    private readonly IVersionRepository _versionRepository;
    private readonly IStorageRepository _storageRepository;
    private readonly IEncryptionService _encryptionService;

    public KeyService(
        IKeyRepository keyRepository,
        IVersionRepository versionRepository,
        IStorageRepository storageRepository,
        IEncryptionService encryptionService)
    {
        _keyRepository = keyRepository;
        _versionRepository = versionRepository;
        _storageRepository = storageRepository;
        _encryptionService = encryptionService;
    }

    public async Task<Key> CreateKeyAsync(int storageId, string keyName, string value, string createdBy)
    {
        // Validate storage exists
        var storage = await _storageRepository.GetByIdAsync(storageId);
        if (storage == null)
            throw new InvalidOperationException("Storage not found");

        // Check if key already exists
        if (await _keyRepository.ExistsAsync(storageId, keyName))
            throw new InvalidOperationException($"Key '{keyName}' already exists in storage");

        // Derive encryption key from storage token
        var encryptionKey = _encryptionService.DeriveKey(storage.AccessToken, storage.Salt);

        // Create key
        var key = new Key
        {
            StorageId = storageId,
            KeyName = keyName,
            CreatedAt = DateTime.UtcNow
        };
        key = await _keyRepository.CreateAsync(key);

        // Create first version with encrypted value
        var (encryptedValue, iv) = _encryptionService.Encrypt(value, encryptionKey);
        var version = new VersionEntry
        {
            KeyId = key.Id,
            VersionNumber = 1,
            EncryptedValue = encryptedValue,
            IV = iv,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        await _versionRepository.CreateAsync(version);

        return key;
    }

    public async Task<(Key Key, bool Created)> UpsertKeyAsync(int storageId, string keyName, string value, string createdBy)
    {
        if (await _keyRepository.ExistsAsync(storageId, keyName))
        {
            var updated = await UpdateKeyAsync(storageId, keyName, value, createdBy);
            return (updated!, false);
        }
        var created = await CreateKeyAsync(storageId, keyName, value, createdBy);
        return (created, true);
    }

    public async Task<Key?> UpdateKeyAsync(int storageId, string keyName, string value, string createdBy)
    {
        // Validate storage exists
        var storage = await _storageRepository.GetByIdAsync(storageId);
        if (storage == null)
            throw new InvalidOperationException("Storage not found");

        // Get existing key
        var key = await _keyRepository.GetByStorageAndKeyNameAsync(storageId, keyName);
        if (key == null)
            throw new InvalidOperationException($"Key '{keyName}' not found");

        // Derive encryption key from storage token
        var encryptionKey = _encryptionService.DeriveKey(storage.AccessToken, storage.Salt);

        // Get next version number
        var nextVersion = await _versionRepository.GetNextVersionNumberAsync(key.Id);

        // Create new version with encrypted value
        var (encryptedValue, iv) = _encryptionService.Encrypt(value, encryptionKey);
        var version = new VersionEntry
        {
            KeyId = key.Id,
            VersionNumber = nextVersion,
            EncryptedValue = encryptedValue,
            IV = iv,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        await _versionRepository.CreateAsync(version);

        return key;
    }

    public async Task<string?> GetKeyValueAsync(int storageId, string keyName, string token)
    {
        // Validate storage exists and token matches
        var storage = await _storageRepository.GetByIdAsync(storageId);
        if (storage == null || storage.AccessToken != token)
            return null;

        // Get key
        var key = await _keyRepository.GetByStorageAndKeyNameAsync(storageId, keyName);
        if (key == null)
            return null;

        // Get latest version
        var versions = await _versionRepository.GetByKeyIdAsync(key.Id);
        var latestVersion = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
        if (latestVersion == null)
            return null;

        // Derive encryption key and decrypt
        var encryptionKey = _encryptionService.DeriveKey(storage.AccessToken, storage.Salt);
        var decryptedValue = _encryptionService.Decrypt(latestVersion.EncryptedValue, latestVersion.IV, encryptionKey);

        return decryptedValue;
    }

    public async Task<IEnumerable<Key>> GetKeysByStorageIdAsync(int storageId)
    {
        return await _keyRepository.GetByStorageIdAsync(storageId);
    }

    public async Task DeleteKeyAsync(int storageId, string keyName)
    {
        var key = await _keyRepository.GetByStorageAndKeyNameAsync(storageId, keyName);
        if (key == null)
            throw new InvalidOperationException($"Key '{keyName}' not found");

        await _keyRepository.DeleteAsync(key.Id);
    }

    public async Task<VersionEntry?> GetKeyVersionAsync(int storageId, string keyName, int versionNumber)
    {
        var key = await _keyRepository.GetByStorageAndKeyNameAsync(storageId, keyName);
        if (key == null)
            return null;

        return await _versionRepository.GetByKeyAndVersionAsync(key.Id, versionNumber);
    }

    public async Task<IEnumerable<VersionEntry>> GetKeyHistoryAsync(int storageId, string keyName)
    {
        var key = await _keyRepository.GetByStorageAndKeyNameAsync(storageId, keyName);
        if (key == null)
            return Enumerable.Empty<VersionEntry>();

        return await _versionRepository.GetByKeyIdAsync(key.Id);
    }
}