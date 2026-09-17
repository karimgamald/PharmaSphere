using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Domain.Entities;
using System.Reflection;

namespace PharmaSphere.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================
        // Medicines & Inventory
        // =========================
        //public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<MedicineBatch> MedicineBatches { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<DrugInteraction> DrugInteractions { get; set; }

        public DbSet<StockReservation> StockReservations { get; set; }

        public DbSet<StockTransaction> StockTransactions { get; set; }


        // =========================
        // Purchasing
        // =========================

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }


        // =========================
        // Sales & Orders
        // =========================

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<SalesReturn> SalesReturns { get; set; }


        // =========================
        // Prescriptions
        // =========================

        public DbSet<Prescription> Prescriptions { get; set; }


        // =========================
        // Payments
        // =========================

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configure ASP.NET Core Identity
            base.OnModelCreating(builder);

            // Apply all IEntityTypeConfiguration<T>
            // classes from this assembly.
            builder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly());
        }
    }
}