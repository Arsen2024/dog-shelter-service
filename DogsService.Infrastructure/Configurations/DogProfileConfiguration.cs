using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;
public class DogProfileConfiguration : IEntityTypeConfiguration<DogProfileEntity>
{
    public void Configure(EntityTypeBuilder<DogProfileEntity> builder)
    {
        builder.ToTable("DogProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Gender)
            .HasMaxLength(20);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Temperament)
            .HasMaxLength(500);

        builder.HasIndex(p => p.DogId)
            .IsUnique();
    }
}
