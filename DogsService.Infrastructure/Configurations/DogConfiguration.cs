using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DogsService.Domain.Entities;
using DogsService.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Infrastructure.Configurations;

public class DogConfiguration : IEntityTypeConfiguration<DogEntity>
{
    public void Configure(EntityTypeBuilder<DogEntity> builder)
    {
        builder.ToTable("Dogs", table =>
        {
            table.HasCheckConstraint(
                "CK_Dogs_TailLength",
                "[TailLength] >= 0");

            table.HasCheckConstraint(
                "CK_Dogs_Weight",
                "[Weight] > 0");

            table.HasCheckConstraint(
                "CK_Dogs_Status",
                "[Status] IN ('Available', 'Reserved', 'Adopted', 'UnderTreatment')");
        });

        // Primary key
        builder.HasKey(d => d.Id);

        // Properties
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Color)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.TailLength)
            .IsRequired();

        builder.Property(d => d.Weight)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(DogStatus.Available);

        // Audit fields
        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(d => d.UpdatedAt);

        builder.Property(d => d.CreatedBy)
            .HasMaxLength(100);

        builder.Property(d => d.UpdatedBy)
            .HasMaxLength(100);

        // Soft delete
        builder.Property(d => d.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(d => d.DeletedAt);

        // Optimistic concurrency
        builder.Property(d => d.RowVersion)
            .IsRowVersion();

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

        // Indexes
        builder.HasIndex(d => d.BreedId);

        builder.HasIndex(d => d.KennelId);

        builder.HasIndex(d => d.Status);

        builder.HasIndex(d => d.IsDeleted);

        // Global query filter for soft delete
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
