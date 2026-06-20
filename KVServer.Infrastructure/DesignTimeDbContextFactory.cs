using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using KVServer.Infrastructure.Data;

namespace KVServer.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KVServerDbContext>
{
    public KVServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KVServerDbContext>();
        optionsBuilder.UseSqlite("Data Source=./data/kvserver.db");

        return new KVServerDbContext(optionsBuilder.Options);
    }
}