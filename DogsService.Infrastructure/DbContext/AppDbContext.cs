using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Infrastructure.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly ICurrentUser? _currentUser;

    // currentUser необов'язковий: design-time фабрика міграцій створює контекст без нього
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<DogEntity> Dogs => Set<DogEntity>();
    public DbSet<BreedEntity> Breeds => Set<BreedEntity>();
    public DbSet<DogProfileEntity> DogProfiles => Set<DogProfileEntity>();
    public DbSet<KennelEntity> Kennels => Set<KennelEntity>();
    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();
    public DbSet<DogFeatureEntity> DogFeatures => Set<DogFeatureEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var now = DateTime.UtcNow;
        var user = _currentUser?.UserName ?? "system";

        foreach (var entry in ChangeTracker.Entries<DogEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                        entry.Entity.CreatedAt = now;

                    entry.Entity.CreatedBy ??= user;
                    entry.Entity.IsDeleted = false;
                    entry.Entity.DeletedAt = null;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;

                    entry.Property(d => d.CreatedAt).IsModified = false;
                    entry.Property(d => d.CreatedBy).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // soft delete
                    entry.State = EntityState.Modified;

                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;
                    break;
            }
        }
    }
}
