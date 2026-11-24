using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;

public class BreedConfiguration : IEntityTypeConfiguration<BreedEntity>
{
    public void Configure(EntityTypeBuilder<BreedEntity> builder)
    {
        builder.ToTable("Breeds");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Description).HasMaxLength(500);

        builder.HasIndex(b => b.Name).IsUnique();

        // Довідники — статично через HasData з фіксованими Id (анонімні типи, бо setter-и приватні)
        builder.HasData(
            new { Id = SeedIds.Labrador, Name = "Labrador Retriever", Description = (string?)"Friendly and intelligent dog breed." },
            new { Id = SeedIds.GermanShepherd, Name = "German Shepherd", Description = (string?)"Intelligent and versatile working dog breed." },
            new { Id = SeedIds.Beagle, Name = "Beagle", Description = (string?)"Friendly and curious hunting dog breed." },
            new { Id = SeedIds.Mixed, Name = "Mixed Breed", Description = (string?)"Dog with mixed ancestry." });
    }
}
