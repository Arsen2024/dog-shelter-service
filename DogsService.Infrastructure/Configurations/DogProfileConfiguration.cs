using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;

public class DogProfileConfiguration : IEntityTypeConfiguration<DogProfileEntity>
{
    public void Configure(EntityTypeBuilder<DogProfileEntity> builder)
    {
        builder.ToTable("DogProfiles", table =>
        {
            table.HasCheckConstraint("CK_DogProfiles_Gender",
                "[Gender] IS NULL OR [Gender] IN ('Male', 'Female', 'Unknown')");
        });

        // 1:1 — FK є одночасно PK залежної таблиці
        builder.HasKey(p => p.DogId);

        builder.Property(p => p.Gender).HasMaxLength(20);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Temperament).HasMaxLength(500);

        // Той самий soft-delete фільтр, що й у батьківської сутності
        builder.HasQueryFilter(p => !p.Dog.IsDeleted);

        builder.HasData(
            new DogProfileEntity
            {
                DogId = SeedIds.Rex, BirthDate = new DateTime(2022, 3, 14), Gender = "Male",
                Description = "Energetic retriever, loves water.", Temperament = "Playful, friendly"
            },
            new DogProfileEntity
            {
                DogId = SeedIds.Bella, BirthDate = new DateTime(2021, 7, 2), Gender = "Female",
                Description = "Calm and attentive shepherd.", Temperament = "Loyal, calm"
            },
            new DogProfileEntity
            {
                DogId = SeedIds.Max, BirthDate = new DateTime(2023, 1, 20), Gender = "Male",
                Description = "Curious beagle with a great nose.", Temperament = "Curious, social"
            },
            new DogProfileEntity
            {
                DogId = SeedIds.Luna, BirthDate = new DateTime(2020, 11, 5), Gender = "Female",
                Description = "Shy mixed-breed, recovering after surgery.", Temperament = "Shy, gentle"
            },
            new DogProfileEntity
            {
                DogId = SeedIds.Charlie, BirthDate = new DateTime(2019, 5, 30), Gender = "Male",
                Description = "Adopted family dog.", Temperament = "Calm, affectionate"
            });
    }
}
