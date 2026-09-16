using PharmaSphere.Domain.Enums;
using System;

namespace PharmaSphere.Domain.Entities
{
    public class StockTransaction
    {
        public int Id { get; set; }

        public int MedicineBatchId { get; set; }
        public MedicineBatch MedicineBatch { get; set; } = null!;

        public StockTransactionType Type { get; set; }

        public int Quantity { get; set; }

        public int BalanceAfterTransaction { get; set; }

        public string? Reference { get; set; }

        public string? Notes { get; set; }

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}