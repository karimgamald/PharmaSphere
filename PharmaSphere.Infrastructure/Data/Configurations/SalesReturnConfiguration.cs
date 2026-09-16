using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

public class SalesReturnConfiguration
    : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(
        EntityTypeBuilder<SalesReturn> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OrderItem)
            .WithMany()
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MedicineBatch)
            .WithMany()
            .HasForeignKey(x => x.MedicineBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.RefundAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);
    }
}