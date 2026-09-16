using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.HasKey(po => po.Id);

            builder.Property(po => po.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(po => po.TotalAmount)
                .HasPrecision(18, 2);

            builder.HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(po => po.InvoiceNumber)
                .IsUnique();

            builder.HasIndex(po => po.OrderDate);
        }
    }
}