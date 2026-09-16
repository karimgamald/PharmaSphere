using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class SalesReturn
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        public int MedicineBatchId { get; set; }
        public MedicineBatch MedicineBatch { get; set; } = null!;

        public int ReturnedUnits { get; set; } // بالوحدات الصغرى
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ReturnedAt { get; set; } = DateTime.UtcNow;
    }
}
