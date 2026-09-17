using Microsoft.EntityFrameworkCore.Storage;
using PharmaSphere.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace PharmaSphere.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Medicine> Medicines { get; }
        IGenericRepository<AuditLog> AuditLogs { get; }
        IGenericRepository<MedicineBatch> MedicineBatches { get; }

        IGenericRepository<Category> Categories { get; }

        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }

        IGenericRepository<PurchaseOrder> PurchaseOrders { get; }
        IGenericRepository<PurchaseOrderItem> PurchaseOrderItems { get; }

        IGenericRepository<Supplier> Suppliers { get; }

        IGenericRepository<Prescription> Prescriptions { get; }
        IGenericRepository<PaymentTransaction> PaymentTransactions { get; }

        IGenericRepository<DrugInteraction> DrugInteractions { get; }

        IGenericRepository<StockReservation> StockReservations { get; }

        IGenericRepository<SalesReturn> SalesReturns { get; }

        IGenericRepository<StockTransaction> StockTransactions { get; }

        // Database Transactions
        Task<IDbContextTransaction> BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        // Save changes
        Task<int> CompleteAsync();
    }
}
