using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
    {
        public void Configure(EntityTypeBuilder<Medicine> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(m => m.ScientificName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.ActiveIngredient)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(m => m.ActiveIngredientConcentration)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Barcode)
                .HasMaxLength(100);

            builder.Property(m => m.ImagePath)
                .HasMaxLength(500);

            builder.Property(m => m.UnitsPerBox)
                .HasDefaultValue(1);

            // Barcode should be unique if it exists
            builder.HasIndex(m => m.Barcode)
                .IsUnique()
                .HasFilter("[Barcode] IS NOT NULL");

            builder.HasIndex(m => m.ActiveIngredient);

            builder.HasOne(m => m.Category)
                .WithMany(c => c.Medicines)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.Batches)
                .WithOne(b => b.Medicine)
                .HasForeignKey(b => b.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}