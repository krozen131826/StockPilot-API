using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Persistence.EntityConfiguration;

public class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint(
                "CK_Products_CostPrice",
                "\"CostPrice\" >= 0");

            table.HasCheckConstraint(
                "CK_Products_SellingPrice",
                "\"SellingPrice\" >= 0");

            table.HasCheckConstraint(
                "CK_Products_QuantityOnHand",
                "\"QuantityOnHand\" >= 0");

            table.HasCheckConstraint(
                "CK_Products_ReorderLevel",
                "\"ReorderLevel\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SKU)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.CostPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.SellingPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.QuantityOnHand)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ReorderLevel)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.SKU)
            .IsUnique();

        builder.HasIndex(x => x.Name);

        builder.HasIndex(x => new
        {
            x.IsActive,
            x.QuantityOnHand,
            x.ReorderLevel
        });

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.LastModifiedDate);
    }
}