using DogsService.Domain.Entities;
using DogsService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;

public class DogConfiguration : IEntityTypeConfiguration<DogEntity>
{
    public void Configure(EntityTypeBuilder<DogEntity> builder)
    {
        builder.ToTable("Dogs", table =>
        {
            table.HasCheckConstraint("CK_Dogs_TailLength", "[TailLength] >= 0");
            table.HasCheckConstraint("CK_Dogs_Weight", "[Weight] > 0");
            table.HasCheckConstraint("CK_Dogs_Status",
                "[Status] IN ('Available', 'Reserved', 'Adopted', 'UnderTreatment')");
        });

        // PK: GUID — ідентифікатори генеруються застосунком і синхронізуються
        // з іншими сервісами (Adoptions, Health), тому не IDENTITY.
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Color).IsRequired().HasMaxLength(100);
        builder.Property(d => d.TailLength).IsRequired();
        builder.Property(d => d.Weight).HasPrecision(6, 2).IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(DogStatus.Available);

        // Audit
        builder.Property(d => d.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(d => d.UpdatedAt);
        builder.Property(d => d.CreatedBy).HasMaxLength(100);
        builder.Property(d => d.UpdatedBy).HasMaxLength(100);

        // Soft delete
        builder.Property(d => d.IsDeleted).HasDefaultValue(false);
        builder.Property(d => d.DeletedAt);

        // Optimistic concurrency
        builder.Property(d => d.RowVersion).IsRowVersion();

        // Relationships
        builder.HasOne(d => d.Breed)
            .WithMany(b => b.Dogs)
            .HasForeignKey(d => d.BreedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Kennel)
            .WithMany(k => k.Dogs)
            .HasForeignKey(d => d.KennelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Profile)
            .WithOne(p => p.Dog)
            .HasForeignKey<DogProfileEntity>(p => p.DogId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.DogFeatures)
            .WithOne(df => df.Dog)
            .HasForeignKey(df => df.DogId)
            .OnDelete(DeleteBehavior.Cascade);

        // Індекси (усі фільтровані — лише для "живих" записів)

        // Запит: перевірка дубліката імені при створенні + пошук за іменем
        builder.HasIndex(d => d.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Dogs_Name_Active");

        // Запит: GET /dogs?breed=X&status=Available (список собак породи за статусом)
        builder.HasIndex(d => new { d.BreedId, d.Status })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Dogs_BreedId_Status_Active");

        // Запит: "собаки у вольєрі K" (заповненість вольєра)
        builder.HasIndex(d => d.KennelId)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Dogs_KennelId_Active");

        // Запит: "усі доступні для прилаштування" (фільтр за статусом)
        builder.HasIndex(d => d.Status)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Dogs_Status_Active");

        builder.HasQueryFilter(d => !d.IsDeleted);

        // Seed
        builder.HasData(
            new DogEntity
            {
                Id = SeedIds.Rex, Name = "Rex", Color = "golden", TailLength = 35, Weight = 29.50m,
                BreedId = SeedIds.Labrador, KennelId = SeedIds.KennelA, Status = DogStatus.Available,
                CreatedAt = SeedIds.SeedDate, CreatedBy = "seed", IsDeleted = false
            },
            new DogEntity
            {
                Id = SeedIds.Bella, Name = "Bella", Color = "black & tan", TailLength = 40, Weight = 27.00m,
                BreedId = SeedIds.GermanShepherd, KennelId = SeedIds.KennelB, Status = DogStatus.Available,
                CreatedAt = SeedIds.SeedDate, CreatedBy = "seed", IsDeleted = false
            },
            new DogEntity
            {
                Id = SeedIds.Max, Name = "Max", Color = "brown & white", TailLength = 25, Weight = 12.30m,
                BreedId = SeedIds.Beagle, KennelId = SeedIds.KennelA, Status = DogStatus.Reserved,
                CreatedAt = SeedIds.SeedDate, CreatedBy = "seed", IsDeleted = false
            },
            new DogEntity
            {
                Id = SeedIds.Luna, Name = "Luna", Color = "grey", TailLength = 20, Weight = 9.80m,
                BreedId = SeedIds.Mixed, KennelId = SeedIds.KennelC, Status = DogStatus.UnderTreatment,
                CreatedAt = SeedIds.SeedDate, CreatedBy = "seed", IsDeleted = false
            },
            new DogEntity
            {
                Id = SeedIds.Charlie, Name = "Charlie", Color = "black", TailLength = 33, Weight = 31.20m,
                BreedId = SeedIds.Labrador, KennelId = null, Status = DogStatus.Adopted,
                CreatedAt = SeedIds.SeedDate, CreatedBy = "seed", IsDeleted = false
            });
    }
}
