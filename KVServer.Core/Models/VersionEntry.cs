namespace KVServer.Core.Models;

public class VersionEntry
{
    public int Id { get; set; }
    public int KeyId { get; set; }
    public int VersionNumber { get; set; }
    public string EncryptedValue { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation property
    public Key Key { get; set; } = null!;
}