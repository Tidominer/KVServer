namespace KVServer.Core.Models;

public class Storage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Models.Key> Keys { get; set; } = new List<Models.Key>();
}