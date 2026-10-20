
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Persistence.EntityConfiguration;

public class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", table =>
        {
            table.HasCheckConstraint(
                "CK_Categories_Name_NotEmpty",
                "\"Name\" <> ''");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasMany(x => x.Products)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.LastModifiedDate);
    }
}