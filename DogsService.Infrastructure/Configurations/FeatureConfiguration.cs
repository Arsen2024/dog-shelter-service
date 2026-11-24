using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<FeatureEntity>
{
    public void Configure(EntityTypeBuilder<FeatureEntity> builder)
    {
        builder.ToTable("Features");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Description).HasMaxLength(500);

        builder.HasIndex(f => f.Name).IsUnique();

        builder.HasData(
            new { Id = SeedIds.Friendly, Name = "Friendly", Description = (string?)"Friendly towards people and other animals." },
            new { Id = SeedIds.Trained, Name = "Trained", Description = (string?)"Has completed basic obedience training." },
            new { Id = SeedIds.GoodWithChildren, Name = "Good with children", Description = (string?)"Comfortable interacting with children." },
            new { Id = SeedIds.Active, Name = "Active", Description = (string?)"Has a high activity level and enjoys exercise." });
    }
}
