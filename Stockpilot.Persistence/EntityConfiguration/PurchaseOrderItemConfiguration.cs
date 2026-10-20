using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Persistence.EntityConfiguration;


public class PurchaseOrderItemConfiguration
    : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(
        EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems", table =>
        {
            table.HasCheckConstraint(
                "CK_PurchaseOrderItems_QuantityOrdered",
                "\"QuantityOrdered\" > 0");

            table.HasCheckConstraint(
                "CK_PurchaseOrderItems_QuantityReceived",
                "\"QuantityReceived\" >= 0 AND " +
                "\"QuantityReceived\" <= \"QuantityOrdered\"");

            table.HasCheckConstraint(
                "CK_PurchaseOrderItems_UnitCost",
                "\"UnitCost\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuantityOrdered)
            .IsRequired();

        builder.Property(x => x.QuantityReceived)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.UnitCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.PurchaseOrderId,
            x.ProductId
        })
        .IsUnique();

        builder.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PurchaseOrderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.PurchaseOrderItems)
            .HasForeignKey(x => x.ProductId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}