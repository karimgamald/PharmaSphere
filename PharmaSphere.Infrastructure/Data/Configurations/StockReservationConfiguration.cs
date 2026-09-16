using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
    {
        public void Configure(EntityTypeBuilder<StockReservation> builder)
        {
            builder.HasKey(sr => sr.Id);

            builder.Property(sr => sr.SessionOrUserId)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(sr => sr.ExpirationTime);

            builder.HasIndex(sr => new
            {
                sr.SessionOrUserId,
                sr.MedicineId
            });

            builder.HasOne(sr => sr.Medicine)
                .WithMany()
                .HasForeignKey(sr => sr.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Order)
                .WithMany()
                .HasForeignKey(sr => sr.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}