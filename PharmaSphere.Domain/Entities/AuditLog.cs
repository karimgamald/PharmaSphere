using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        public string? Description { get; set; }

        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
