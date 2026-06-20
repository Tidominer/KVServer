using Microsoft.EntityFrameworkCore;
using KVServer.Core.Models;

namespace KVServer.Infrastructure.Data;

public class KVServerDbContext : DbContext
{
    public KVServerDbContext(DbContextOptions<KVServerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Storage> Storages { get; set; }
    public DbSet<Key> Keys { get; set; }
    public DbSet<VersionEntry> Versions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Storage configuration
        modelBuilder.Entity<Storage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.AccessToken).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AccessToken).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Salt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
        });

        // Key configuration
        modelBuilder.Entity<Key>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StorageId);
            entity.HasIndex(e => new { e.StorageId, e.KeyName }).IsUnique();
            entity.Property(e => e.StorageId).IsRequired();
            entity.Property(e => e.KeyName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Storage)
                .WithMany(s => s.Keys)
                .HasForeignKey(e => e.StorageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // VersionEntry configuration
        modelBuilder.Entity<VersionEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.KeyId);
            entity.HasIndex(e => e.CreatedAt).IsDescending();
            entity.HasIndex(e => new { e.KeyId, e.VersionNumber }).IsUnique();
            entity.Property(e => e.KeyId).IsRequired();
            entity.Property(e => e.VersionNumber).IsRequired();
            entity.Property(e => e.EncryptedValue).IsRequired();
            entity.Property(e => e.IV).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(500);

            entity.HasOne(e => e.Key)
                .WithMany(k => k.Versions)
                .HasForeignKey(e => e.KeyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}