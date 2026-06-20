using System.ComponentModel.DataAnnotations;

namespace KVServer.Core.Models;

public class Key
{
    public int Id { get; set; }
    public int StorageId { get; set; }
    [MaxLength(500)]
    public string KeyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Storage Storage { get; set; } = null!;
    public ICollection<VersionEntry> Versions { get; set; } = new List<VersionEntry>();
}