using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KVServer.Core.Models;
using KVServer.Core.Services;

namespace KVServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KeysController : ControllerBase
{
    private readonly IKeyService _keyService;
    private readonly IStorageService _storageService;
    private readonly ILogger<KeysController> _logger;

    public KeysController(IKeyService keyService, IStorageService storageService, ILogger<KeysController> logger)
    {
        _keyService = keyService;
        _storageService = storageService;
        _logger = logger;
    }

    private Storage GetCurrentStorage()
    {
        return (Storage)HttpContext.Items["Storage"]!;
    }

    private static string TruncateToken(string token, int maxLength = 8)
    {
        if (string.IsNullOrEmpty(token)) return "Empty";
        return token.Length > maxLength ? $"{token.Substring(0, maxLength)}..." : token;
    }

    // GET /api/keys - List all keys in storage
    [HttpGet]
    public async Task<IActionResult> GetKeys()
    {
        try
        {
            var storage = GetCurrentStorage();
            var keys = await _keyService.GetKeysByStorageIdAsync(storage.Id);

            var result = keys.Select(k =>
            {
                var versionCount = k.Versions?.Count ?? 0;
                var lastModified = k.Versions?.Any() == true
                    ? k.Versions.Max(v => v.CreatedAt).ToString("o")
                    : k.CreatedAt.ToString("o");
                return new KeyInfo(k.KeyName, versionCount, lastModified);
            });

            return Ok(new KeyListResponse(result.ToList()));
        }
        catch (Exception ex)
        {
            var storage = GetCurrentStorage();
            _logger.LogError("API Exception: GET /api/keys, Storage: '{StorageName}', Token: '{TokenTruncated}', Error: {ErrorMessage}",
                storage.Name, TruncateToken(storage.AccessToken), ex.Message);
            return StatusCode(500, new { error = "An error occurred while retrieving keys" });
        }
    }

    // POST /api/keys - Create new key-value pair
    [HttpPost]
    public async Task<IActionResult> CreateKey([FromBody] CreateKeyRequest request)
    {
        try
        {
            var storage = GetCurrentStorage();
            await _keyService.CreateKeyAsync(storage.Id, request.Key, request.Value, "web-ui");

            return CreatedAtAction(nameof(GetKey), new { key = request.Key }, new KeyResponse(
                request.Key,
                1,
                DateTime.UtcNow.ToString("o")
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            var storage = GetCurrentStorage();
            _logger.LogError("API Exception: POST /api/keys, Storage: '{StorageName}', Token: '{TokenTruncated}', Key: '{Key}', Error: {ErrorMessage}",
                storage.Name, TruncateToken(storage.AccessToken), request.Key, ex.Message);
            return StatusCode(500, new { error = "An error occurred while creating key" });
        }
    }

    // GET /api/keys/{key} - Get current value of a key
    [HttpGet("{key}")]
    public async Task<IActionResult> GetKey(string key)
    {
        try
        {
            var storage = GetCurrentStorage();
            var value = await _keyService.GetKeyValueAsync(storage.Id, key, storage.AccessToken);

            if (value == null)
                return NotFound(new { error = "Key not found" });

            var keyObj = await _keyService.GetKeysByStorageIdAsync(storage.Id);
            var keyInfo = keyObj.FirstOrDefault(k => k.KeyName == key);

            var lastModified = keyInfo?.Versions?.Any() == true
                ? keyInfo.Versions.Max(v => v.CreatedAt).ToString("o")
                : DateTime.UtcNow.ToString("o");

            return Ok(new KeyValueResponse(
                key,
                value,
                keyInfo?.Versions?.Count ?? 0,
                lastModified
            ));
        }
        catch (Exception ex)
        {
            var storage = GetCurrentStorage();
            _logger.LogError("API Exception: GET /api/keys/{Key}, Storage: '{StorageName}', Token: '{TokenTruncated}', Error: {ErrorMessage}",
                storage.Name, TruncateToken(storage.AccessToken), key, ex.Message);
            return StatusCode(500, new { error = "An error occurred while retrieving key" });
        }
    }

    // PUT /api/keys/{key} - Update key value (creates new version)
    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateKey(string key, [FromBody] UpdateKeyRequest request)
    {
        try
        {
            var storage = GetCurrentStorage();
            await _keyService.UpdateKeyAsync(storage.Id, key, request.Value, "web-ui");

            var keyObj = await _keyService.GetKeysByStorageIdAsync(storage.Id);
            var keyInfo = keyObj.FirstOrDefault(k => k.KeyName == key);

            return Ok(new KeyResponse(
                key,
                keyInfo?.Versions?.Count ?? 0,
                DateTime.UtcNow.ToString("o")
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            var storage = GetCurrentStorage();
            _logger.LogError("API Exception: PUT /api/keys/{Key}, Storage: '{StorageName}', Token: '{TokenTruncated}', Error: {ErrorMessage}",
                storage.Name, TruncateToken(storage.AccessToken), key, ex.Message);
            return StatusCode(500, new { error = "An error occurred while updating key" });
        }
    }

    // DELETE /api/keys/{key} - Delete key and all versions
    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteKey(string key)
    {
        try
        {
            var storage = GetCurrentStorage();
            await _keyService.DeleteKeyAsync(storage.Id, key);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            var storage = GetCurrentStorage();
            _logger.LogError("API Exception: DELETE /api/keys/{Key}, Storage: '{StorageName}', Token: '{TokenTruncated}', Error: {ErrorMessage}",
                storage.Name, TruncateToken(storage.AccessToken), key, ex.Message);
            return StatusCode(500, new { error = "An error occurred while deleting key" });
        }
    }

    // GET /api/keys/{key}/history - Get version history
    [HttpGet("{key}/history")]
    public async Task<IActionResult> GetKeyHistory(string key)
    {
        try
        {
            var storage = GetCurrentStorage();
            var versions = await _keyService.GetKeyHistoryAsync(storage.Id, key);

            if (!versions.Any())
                return NotFound(new { error = "Key not found" });

            var encryptionKey = await Task.Run(() =>
            {
                var encryptionService = new KVServer.Infrastructure.Services.EncryptionService();
                return encryptionService.DeriveKey(storage.AccessToken, storage.Salt);
            });

            var encryptionService = new KVServer.Infrastructure.Services.EncryptionService();
            var result = versions.Select(v => new VersionInfo(
                v.VersionNumber,
                encryptionService.Decrypt(v.EncryptedValue, v.IV, encryptionKey),
                v.CreatedAt.ToString("o")
            ));

            return Ok(new VersionHistoryResponse(result.ToList()));
        }
        catch (Exception ex)
        {
            var storage = GetCurrentStorage();
            _logger.LogError("API Exception: GET /api/keys/{Key}/history, Storage: '{StorageName}', Token: '{TokenTruncated}', Error: {ErrorMessage}",
                storage.Name, TruncateToken(storage.AccessToken), key, ex.Message);
            return StatusCode(500, new { error = "An error occurred while retrieving history" });
        }
    }

    // GET /api/keys/{key}/versions/{version} - Get specific version
    [HttpGet("{key}/versions/{version}")]
    public async Task<IActionResult> GetKeyVersion(string key, int version)
    {
        try
        {
            var storage = GetCurrentStorage();
            var versionEntry = await _keyService.GetKeyVersionAsync(storage.Id, key, version);

            if (versionEntry == null)
                return NotFound(new { error = "Version not found" });

            var encryptionService = new KVServer.Infrastructure.Services.EncryptionService();
            var encryptionKey = encryptionService.DeriveKey(storage.AccessToken, storage.Salt);
            var decryptedValue = encryptionService.Decrypt(versionEntry.EncryptedValue, versionEntry.IV, encryptionKey);

            return Ok(new VersionResponse(
                key,
                version,
                decryptedValue,
                versionEntry.CreatedAt.ToString("o")
            ));
        }
        catch (Exception ex)
        {
            var storage = GetCurrentStorage();
            _logger.LogError("API Exception: GET /api/keys/{Key}/versions/{Version}, Storage: '{StorageName}', Token: '{TokenTruncated}', Error: {ErrorMessage}",
                storage.Name, TruncateToken(storage.AccessToken), key, version, ex.Message);
            return StatusCode(500, new { error = "An error occurred while retrieving version" });
        }
    }
}

// Request/Response Models
public record CreateKeyRequest(string Key, string Value);
public record UpdateKeyRequest(string Value);
public record KeyListResponse(IList<KeyInfo> Keys);
public record KeyInfo(string Key, int Version, string LastModified);
public record KeyResponse(string Key, int Version, string CreatedAt);
public record KeyValueResponse(string Key, string Value, int Version, string LastModified);
public record VersionHistoryResponse(IList<VersionInfo> History);
public record VersionInfo(int Version, string Value, string CreatedAt);
public record VersionResponse(string Key, int Version, string Value, string CreatedAt);