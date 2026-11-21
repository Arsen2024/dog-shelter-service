using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;

public class DogFeatureConfiguration : IEntityTypeConfiguration<DogFeatureEntity>
{
    public void Configure(EntityTypeBuilder<DogFeatureEntity> builder)
    {
        builder.ToTable("DogFeatures");

        builder.HasKey(df => new { df.DogId, df.FeatureId });

        builder.Property(df => df.AssignedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(df => df.Notes).HasMaxLength(500);

        builder.HasOne(df => df.Dog)
            .WithMany(d => d.DogFeatures)
            .HasForeignKey(df => df.DogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(df => df.Feature)
            .WithMany(f => f.DogFeatures)
            .HasForeignKey(df => df.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);

        // Запит: "усі собаки з характеристикою X"
        builder.HasIndex(df => df.FeatureId);

        builder.HasQueryFilter(df => !df.Dog.IsDeleted);

        builder.HasData(
            new DogFeatureEntity { DogId = SeedIds.Rex, FeatureId = SeedIds.Friendly, AssignedAt = SeedIds.SeedDate, Notes = "Greets everyone" },
            new DogFeatureEntity { DogId = SeedIds.Rex, FeatureId = SeedIds.Trained, AssignedAt = SeedIds.SeedDate, Notes = "Basic commands" },
            new DogFeatureEntity { DogId = SeedIds.Bella, FeatureId = SeedIds.Trained, AssignedAt = SeedIds.SeedDate, Notes = null },
            new DogFeatureEntity { DogId = SeedIds.Bella, FeatureId = SeedIds.Active, AssignedAt = SeedIds.SeedDate, Notes = "Needs long walks" },
            new DogFeatureEntity { DogId = SeedIds.Max, FeatureId = SeedIds.Friendly, AssignedAt = SeedIds.SeedDate, Notes = null },
            new DogFeatureEntity { DogId = SeedIds.Max, FeatureId = SeedIds.GoodWithChildren, AssignedAt = SeedIds.SeedDate, Notes = "Tested with kids" },
            new DogFeatureEntity { DogId = SeedIds.Luna, FeatureId = SeedIds.Friendly, AssignedAt = SeedIds.SeedDate, Notes = null },
            new DogFeatureEntity { DogId = SeedIds.Charlie, FeatureId = SeedIds.GoodWithChildren, AssignedAt = SeedIds.SeedDate, Notes = null });
    }
}
