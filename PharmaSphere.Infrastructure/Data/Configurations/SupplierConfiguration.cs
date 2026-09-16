using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Infrastructure.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Phone)
                .HasMaxLength(30);

            builder.Property(s => s.Email)
                .HasMaxLength(150);

            builder.Property(s => s.Address)
                .HasMaxLength(200);

            builder.HasIndex(s => s.Name);

            
        }
    }
}