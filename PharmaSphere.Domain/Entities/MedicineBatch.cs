using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class MedicineBatch
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }

        [Range(1, int.MaxValue)]
        public int OriginalUnits { get; set; }

        [Range(0, int.MaxValue)]
        public int RemainingUnits { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
}
