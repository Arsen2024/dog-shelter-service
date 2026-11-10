using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Infrastructure.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DogEntity> Dogs => Set<DogEntity>();
    public DbSet<BreedEntity> Breeds => Set<BreedEntity>();
    public DbSet<DogProfileEntity> DogProfiles => Set<DogProfileEntity>();
    public DbSet<KennelEntity> Kennels => Set<KennelEntity>();
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();
    public DbSet<DogFeatureEntity> DogFeatures => Set<DogFeatureEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries<DogEntity>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.CreatedAt = currentTime;
                    }

                    entry.Entity.IsDeleted = false;
                    entry.Entity.DeletedAt = null;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = currentTime;

                    entry.Property(d => d.CreatedAt).IsModified = false;
                    entry.Property(d => d.CreatedBy).IsModified = false;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;

                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = currentTime;
                    entry.Entity.UpdatedAt = currentTime;
                    break;
            }
        }
    }
}