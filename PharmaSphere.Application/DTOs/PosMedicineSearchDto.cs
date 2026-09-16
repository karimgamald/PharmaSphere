using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Application.DTOs
{
    public class PosMedicineSearchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool RequiresPrescription { get; set; }
    }
}
