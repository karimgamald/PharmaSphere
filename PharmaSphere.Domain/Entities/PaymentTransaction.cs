using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; } = string.Empty; // المعرف القادم من بوابة الدفع
        public decimal Amount { get; set; }
        public string PaymentProvider { get; set; } = string.Empty;      // e.g. "Paymob_Sandbox"
        public bool IsSuccess { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int OrderId { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; } = null!;
    }
}
