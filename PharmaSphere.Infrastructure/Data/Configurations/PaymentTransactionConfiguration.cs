using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.HasKey(pt => pt.Id);

            builder.Property(pt => pt.TransactionReference)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(pt => pt.PaymentProvider)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pt => pt.Amount)
                .HasPrecision(18, 2);

            builder.HasOne(pt => pt.Order)
                .WithOne(o => o.PaymentTransaction)
                .HasForeignKey<PaymentTransaction>(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pt => pt.TransactionReference)
                .IsUnique();

            builder.HasIndex(pt => pt.OrderId)
                .IsUnique();
        }
    }
}