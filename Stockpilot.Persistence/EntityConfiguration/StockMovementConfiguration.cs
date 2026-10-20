using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Persistence.EntityConfiguration;

public class StockMovementConfiguration
    : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements", table =>
        {
            table.HasCheckConstraint(
                "CK_StockMovements_QuantityBefore",
                "\"QuantityBefore\" >= 0");

            table.HasCheckConstraint(
                "CK_StockMovements_QuantityAfter",
                "\"QuantityAfter\" >= 0");

            table.HasCheckConstraint(
                "CK_StockMovements_QuantityChange",
                "\"QuantityChange\" <> 0");

            table.HasCheckConstraint(
                "CK_StockMovements_QuantityConsistency",
                "\"QuantityAfter\" = " +
                "\"QuantityBefore\" + \"QuantityChange\"");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MovementType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.QuantityChange)
            .IsRequired();

        builder.Property(x => x.QuantityBefore)
            .IsRequired();

        builder.Property(x => x.QuantityAfter)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.CreatedDate
        });

        builder.HasIndex(x => new
        {
            x.PurchaseOrderId,
            x.ProductId
        });

        builder.HasOne(x => x.Product)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.ProductId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.PurchaseOrderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}