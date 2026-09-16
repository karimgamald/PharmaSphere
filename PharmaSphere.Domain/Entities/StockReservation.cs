using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class StockReservation
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public string SessionOrUserId { get; set; } = string.Empty;
        public int ReservedUnits { get; set; }
        public int? OrderId { get; set; }

        public Order? Order { get; set; }

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpirationTime { get; set; }
        public bool IsReleasedOrCompleted { get; set; } = false;
    }
}