using Microsoft.AspNetCore.Identity;
using PharmaSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }

        // إحداثيات الموقع المباشر (للعميل أو الطيار)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Order> ClientOrders { get;set; } = new List<Order>();
        public virtual ICollection<Order> ProcessedOrders { get; set; } = new List<Order>();
        public virtual ICollection<Order> DeliveryOrders { get; set; } = new List<Order>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
