using PharmaSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class Prescription
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = 
string.Empty;
        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedById { get; set; }

        public ApplicationUser? ReviewedBy { get; set; }
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.PendingReview;
        public string? PharmacistNotes { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public string ClientId { get; set; } = string.Empty;
        public int? OrderId { get; set; } // يتم ربطه بالطلب بعد تحويل الروشتة لأدوية

        // Navigation Properties
        public virtual ApplicationUser Client { get; set; } = null!;
        public virtual Order? Order { get; set; }
    }
}
