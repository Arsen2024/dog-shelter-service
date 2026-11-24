using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;

/// <summary>M:N Dog ↔ Feature через явну проміжну сутність із власними полями (AssignedAt, Notes) і складеним ключем.</summary>
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

        // Запит: «собаки з характеристикою X, додані після дати» (фільтр + сортування за AssignedAt)
        builder.HasIndex(df => new { df.FeatureId, df.AssignedAt });

        builder.HasQueryFilter(df => !df.Dog.IsDeleted);
    }
}
