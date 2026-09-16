using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.PharmacistNotes)
                .HasMaxLength(2000);

            builder.Property(p => p.ReviewedById)
                .HasMaxLength(450);

            builder.HasOne(p => p.Client)
                .WithMany(u => u.Prescriptions)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Order)
                .WithOne(o => o.Prescription)
                .HasForeignKey<Prescription>(p => p.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.ReviewedBy)
                .WithMany()
                .HasForeignKey(p => p.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.ClientId);

            builder.HasIndex(p => p.OrderId);
        }
    }
}