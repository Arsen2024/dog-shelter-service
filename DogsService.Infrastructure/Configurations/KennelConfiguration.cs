using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DogsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogsService.Infrastructure.Configurations;
public class KennelConfiguration : IEntityTypeConfiguration<KennelEntity>
{
    public void Configure(EntityTypeBuilder<KennelEntity> builder)
    {
        builder.ToTable("Kennels", table =>
        {
            table.HasCheckConstraint(
                "CK_Kennels_Capacity",
                "[Capacity] > 0");
        });

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(k => k.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(k => k.Capacity)
            .IsRequired();

        builder.HasIndex(k => k.Name)
            .IsUnique();

        builder.HasData(
            new KennelEntity
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Kennel A",
                Location = "Shelter Building 1",
                Capacity = 10
            },
            new KennelEntity
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Kennel B",
                Location = "Shelter Building 2",
                Capacity = 15
            },
            new KennelEntity
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Kennel C",
                Location = "Shelter Building 3",
                Capacity = 8
            }
        );
    }
}
