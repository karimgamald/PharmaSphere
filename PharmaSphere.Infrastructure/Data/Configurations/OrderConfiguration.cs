using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // =========================
            // Primary Key
            // =========================
            builder.HasKey(o => o.Id);


            // =========================
            // Order Number
            // =========================
            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();


            // =========================
            // Money
            // =========================
            builder.Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            builder.Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.DeliveryFee)
                .HasPrecision(18, 2);

            builder.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);


            // =========================
            // Text
            // =========================
            builder.Property(o => o.ShippingAddress)
                .HasMaxLength(500);


            // =========================
            // Client Relationship
            // =========================
            builder.HasOne(o => o.Client)
                .WithMany(u => u.ClientOrders)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Pharmacist Relationship
            // =========================
            builder.HasOne(o => o.Pharmacist)
                .WithMany(u => u.ProcessedOrders)
                .HasForeignKey(o => o.PharmacistId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Delivery Boy Relationship
            // =========================
            builder.HasOne(o => o.DeliveryBoy)
                .WithMany(u => u.DeliveryOrders)
                .HasForeignKey(o => o.DeliveryBoyId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Prescription Relationship
            // =========================
            builder.HasOne(o => o.Prescription)
                .WithOne(p => p.Order)
                .HasForeignKey<Prescription>(p => p.OrderId)
                .OnDelete(DeleteBehavior.SetNull);


            // =========================
            // Payment Transaction
            // =========================
            builder.HasOne(o => o.PaymentTransaction)
                .WithOne(pt => pt.Order)
                .HasForeignKey<PaymentTransaction>(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // Indexes
            // =========================
            builder.HasIndex(o => o.OrderDate);

            builder.HasIndex(o => o.ClientId);

            builder.HasIndex(o => o.OrderStatus);

            builder.HasIndex(o => o.PaymentStatus);
        }
    }
}