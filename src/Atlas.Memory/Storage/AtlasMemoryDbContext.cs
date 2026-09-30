using Atlas.Memory.Models.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Memory.Storage;

/// <summary>
/// Provides Entity Framework Core access to persisted Atlas memories.
/// </summary>
public sealed class AtlasMemoryDbContext(
    DbContextOptions<AtlasMemoryDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the persisted Atlas memories.
    /// </summary>
    public DbSet<AtlasMemoryRecord> Memories =>
        Set<AtlasMemoryRecord>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var memory =
            modelBuilder.Entity<AtlasMemoryRecord>();

        memory.ToTable("Memories");

        memory.HasKey(
            entity => entity.Id);

        memory.Property(
                entity => entity.Content)
            .IsRequired();

        memory.Property(
                entity => entity.CreatedAt)
            .IsRequired();

        memory.Property(
                entity => entity.Type)
            .IsRequired();

        memory.Property(
                entity => entity.InterpretationType)
            .IsRequired(false);

        memory.Property(
                entity => entity.InterpretationData)
            .IsRequired(false);

        memory.HasIndex(
            entity => entity.CreatedAt);

        memory.HasIndex(
            entity => entity.Type);
    }
}
