using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.HasIndex(f => f.Name)
            .IsUnique();

        builder.HasData(
            new FeatureEntity
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Friendly",
                Description = "Friendly towards people and other animals."
            },
            new FeatureEntity
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Trained",
                Description = "Has completed basic obedience training."
            },
            new FeatureEntity
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Good with children",
                Description = "Comfortable interacting with children."
            },
            new FeatureEntity
            {
                Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                Name = "Active",
                Description = "Has a high activity level and enjoys exercise."
            }
        );
    }
}
