
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Persistence.EntityConfiguration;


public class SupplierConfiguration
    : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(150);

        builder.Property(x => x.Email)
            .HasMaxLength(254);

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name);

        builder.HasMany(x => x.PurchaseOrders)
            .WithOne(x => x.Supplier)
            .HasForeignKey(x => x.SupplierId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.LastModifiedDate);
    }
}