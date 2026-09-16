using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
    {
        public void Configure(EntityTypeBuilder<StockTransaction> builder)
        {
            builder.HasKey(st => st.Id);

            builder.Property(st => st.Reference)
                .HasMaxLength(100);

            builder.Property(st => st.Notes)
                .HasMaxLength(500);

            builder.Property(st => st.CreatedById)
                .HasMaxLength(450);

            builder.HasOne(st => st.MedicineBatch)
                .WithMany()
                .HasForeignKey(st => st.MedicineBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(st => st.CreatedBy)
                .WithMany()
                .HasForeignKey(st => st.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(st => st.MedicineBatchId);

            builder.HasIndex(st => st.CreatedAt);

            builder.HasIndex(st => st.Reference);
        }
    }
}