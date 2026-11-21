using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.HasIndex(b => b.Name)
            .IsUnique();

        builder.HasData(
            new BreedEntity
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Labrador Retriever",
                Description = "Friendly and intelligent dog breed."
            },
            new BreedEntity
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "German Shepherd",
                Description = "Intelligent and versatile working dog breed."
            },
            new BreedEntity
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Beagle",
                Description = "Friendly and curious hunting dog breed."
            },
            new BreedEntity
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Mixed Breed",
                Description = "Dog with mixed ancestry."
            }
        );
    }
}
