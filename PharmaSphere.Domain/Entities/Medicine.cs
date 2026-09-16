using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmaSphere.Domain.Entities
{
    public class Medicine
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ScientificName { get; set; } = string.Empty;

        public string ActiveIngredient { get; set; } = string.Empty;

        public string ActiveIngredientConcentration { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        public bool RequiresPrescription { get; set; }

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public int UnitsPerBox { get; set; } = 1;

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<MedicineBatch> Batches { get; set; }
            = new List<MedicineBatch>();

        public virtual ICollection<DrugInteraction> PrimaryInteractions { get; set; }
            = new List<DrugInteraction>();

        public virtual ICollection<DrugInteraction> SecondaryInteractions { get; set; }
            = new List<DrugInteraction>();

        public int TotalStockUnits =>
            Batches?
                .Where(b => b.ExpiryDate > DateTime.UtcNow &&
                            b.RemainingUnits > 0)
                .Sum(b => b.RemainingUnits) ?? 0;
    }
}