using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
        {
            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.BatchNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pi => pi.PurchasePrice)
                .HasPrecision(18, 2);

            builder.Property(pi => pi.SellingPrice)
                .HasPrecision(18, 2);

            builder.Property(pi => pi.TotalPrice)
                .HasPrecision(18, 2);

            builder.HasOne(pi => pi.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(pi => pi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pi => pi.Medicine)
                .WithMany()
                .HasForeignKey(pi => pi.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pi => pi.PurchaseOrderId);

            builder.HasIndex(pi => pi.MedicineId);
        }
    }
}