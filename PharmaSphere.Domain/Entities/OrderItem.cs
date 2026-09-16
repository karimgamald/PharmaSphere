using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int MedicineBatchId { get; set; }
        public MedicineBatch MedicineBatch { get; set; } = null!;

        public decimal UnitCostPrice { get; set; } // سعر التكلفة وقت البيع لحساب الربح اللحظي

        // Foreign Keys
        public int OrderId { get; set; }
        public int MedicineId { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; } = null!;
        public virtual Medicine Medicine { get; set; } = null!;
    }
}
