using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockpilot.Domain.Entities;
namespace Stockpilot.Persistence.EntityConfiguration;



public class PurchaseOrderConfiguration
    : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders", table =>
        {
            table.HasCheckConstraint(
                "CK_PurchaseOrders_Status",
                "\"Status\" IN ('Draft', 'Submitted', 'Received')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.SubmittedAt);

        builder.Property(x => x.ReceivedAt);

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => new
        {
            x.Status,
            x.CreatedDate
        });

        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.PurchaseOrders)
            .HasForeignKey(x => x.SupplierId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.PurchaseOrder)
            .HasForeignKey(x => x.PurchaseOrderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.LastModifiedDate);
    }
}