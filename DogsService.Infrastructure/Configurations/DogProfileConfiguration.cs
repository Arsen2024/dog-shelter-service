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
    }
}
