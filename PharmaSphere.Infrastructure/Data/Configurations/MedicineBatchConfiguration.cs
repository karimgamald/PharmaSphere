using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
    {
        public void Configure(EntityTypeBuilder<MedicineBatch> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.BatchNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(b => b.PurchasePrice)
                .HasPrecision(18, 2);

            builder.Property(b => b.SellingPrice)
                .HasPrecision(18, 2);

            builder.Property(b => b.OriginalUnits)
                .IsRequired();

            builder.Property(b => b.RemainingUnits)
                .IsRequired();

            // Useful for FEFO queries
            builder.HasIndex(b => b.ExpiryDate);

            builder.HasIndex(b => b.BatchNumber);

            builder.HasOne(b => b.Medicine)
                .WithMany(m => m.Batches)
                .HasForeignKey(b => b.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.PurchaseOrder)
                .WithMany(po => po.Batches)
                .HasForeignKey(b => b.PurchaseOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}