using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KVServer.Core.Services;

namespace KVServer.Api.Controllers;

[ApiController]
[Route("api/storages")]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly ILogger<StorageController> _logger;

    public StorageController(IStorageService storageService, ILogger<StorageController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    [HttpGet("current")]
    public IActionResult GetCurrentStorage()
    {
        var storage = HttpContext.Items["Storage"] as KVServer.Core.Models.Storage;
        if (storage == null) return Unauthorized();
        return Ok(new { id = storage.Id, name = storage.Name });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStorage([FromBody] CreateStorageRequest request)
    {
        try
        {
            var storage = await _storageService.CreateStorageAsync(request.Name);
            return Ok(new StorageResponse(
                storage.Id,
                storage.Name,
                storage.AccessToken,
                storage.CreatedAt.ToString("o")
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("API Exception: POST /api/storages, Name: '{StorageName}', Error: {ErrorMessage}",
                request.Name, ex.Message);
            return StatusCode(500, new { error = "An error occurred while creating storage" });
        }
    }
}

public record CreateStorageRequest(string Name);
public record StorageResponse(int Id, string Name, string AccessToken, string CreatedAt);