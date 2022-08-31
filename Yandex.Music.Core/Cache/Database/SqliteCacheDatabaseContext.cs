using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Yandex.Music.Core.Cache.Database;

public class SqliteCacheDatabaseContext : DbContext
{
    public SqliteCacheDatabaseContext(DbContextOptions<SqliteCacheDatabaseContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        EntityTypeBuilder<CacheRecord> cacheTable = modelBuilder.Entity<CacheRecord>();

        cacheTable
            .Property(p => p.CreationTime)
            .HasColumnType("integer")
            .HasConversion(
                v => v.Ticks,
                v => new DateTime(v));

        cacheTable
            .Property(p => p.IsCompressed)
            .HasColumnType("integer")
            .HasConversion(
                v => v ? 1 : 0,
                v => v == 1);
    }
}
