using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class DrugInteractionConfiguration : IEntityTypeConfiguration<DrugInteraction>
    {
        public void Configure(EntityTypeBuilder<DrugInteraction> builder)
        {
            builder.HasKey(di => di.Id);

            builder.Property(di => di.Severity)
                .IsRequired();

            builder.Property(di => di.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(di => di.Medicine)
                .WithMany(m => m.PrimaryInteractions)
                .HasForeignKey(di => di.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(di => di.InteractingMedicine)
                .WithMany(m => m.SecondaryInteractions)
                .HasForeignKey(di => di.InteractingMedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique pair index
            builder.HasIndex(di => new
            {
                di.MedicineId,
                di.InteractingMedicineId
            })
            .IsUnique();
        }
    }
}