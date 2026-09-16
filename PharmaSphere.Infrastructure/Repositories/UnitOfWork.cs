using Microsoft.EntityFrameworkCore.Storage;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace PharmaSphere.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Medicines = new GenericRepository<Medicine>(_context);
            MedicineBatches = new GenericRepository<MedicineBatch>(_context);

            Categories = new GenericRepository<Category>(_context);

            Orders = new GenericRepository<Order>(_context);
            OrderItems = new GenericRepository<OrderItem>(_context);

            PurchaseOrders = new GenericRepository<PurchaseOrder>(_context);
            PurchaseOrderItems = new GenericRepository<PurchaseOrderItem>(_context);

            Suppliers = new GenericRepository<Supplier>(_context);

            Prescriptions = new GenericRepository<Prescription>(_context);
            PaymentTransactions = new GenericRepository<PaymentTransaction>(_context);

            DrugInteractions = new GenericRepository<DrugInteraction>(_context);

            StockReservations = new GenericRepository<StockReservation>(_context);

            SalesReturns = new GenericRepository<SalesReturn>(_context);

            StockTransactions = new GenericRepository<StockTransaction>(_context);
        }

        // =========================
        // Repositories
        // =========================

        public IGenericRepository<Medicine> Medicines { get; private set; }

        public IGenericRepository<MedicineBatch> MedicineBatches { get; private set; }

        public IGenericRepository<Category> Categories { get; private set; }

        public IGenericRepository<Order> Orders { get; private set; }

        public IGenericRepository<OrderItem> OrderItems { get; private set; }

        public IGenericRepository<PurchaseOrder> PurchaseOrders { get; private set; }

        public IGenericRepository<PurchaseOrderItem> PurchaseOrderItems { get; private set; }

        public IGenericRepository<Supplier> Suppliers { get; private set; }

        public IGenericRepository<Prescription> Prescriptions { get; private set; }

        public IGenericRepository<PaymentTransaction> PaymentTransactions { get; private set; }

        public IGenericRepository<DrugInteraction> DrugInteractions { get; private set; }

        public IGenericRepository<StockReservation> StockReservations { get; private set; }

        public IGenericRepository<SalesReturn> SalesReturns { get; private set; }

        public IGenericRepository<StockTransaction> StockTransactions { get; private set; }


        // =========================
        // Transaction
        // =========================

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _currentTransaction =
                await _context.Database.BeginTransactionAsync();

            return _currentTransaction;
        }


        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
                return;

            try
            {
                await _currentTransaction.CommitAsync();
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }


        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
                return;

            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }


        // =========================
        // Save Changes
        // =========================

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }


        // =========================
        // Dispose
        // =========================

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}