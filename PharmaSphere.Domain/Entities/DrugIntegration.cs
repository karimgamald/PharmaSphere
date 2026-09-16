using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Entities
{
    public class DrugInteraction
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public int InteractingMedicineId { get; set; }
        public Medicine InteractingMedicine { get; set; } = null!;

        public string Severity { get; set; } = "High"; // High, Medium, Low
        public string Description { get; set; } = string.Empty;
    }
}