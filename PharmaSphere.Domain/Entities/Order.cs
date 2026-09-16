using PharmaSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();

        public OrderType OrderType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // بيانات التوصيل للطلبات أونلاين
        public string? ShippingAddress { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }

        // Foreign Keys (Nullable لدعم مبيعات الـ POS للعملاء الزائرين)
        public string? ClientId { get; set; }
        public string? PharmacistId { get; set; }
        public string? DeliveryBoyId { get; set; }

        // Navigation Properties
        public virtual ApplicationUser? Client { get; set; }
        public virtual ApplicationUser? Pharmacist { get; set; }
        public virtual ApplicationUser? DeliveryBoy { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual Prescription? Prescription { get; set; }
        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }
}
