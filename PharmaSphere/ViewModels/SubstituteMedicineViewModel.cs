namespace PharmaSphere.Web.ViewModels
{
    public class SubstituteMedicineViewModel 
    { 
        public int MedicineId { get; set; } 
        public string Name { get; set; } = string.Empty; 
        public string? ScientificName { get; set; } 
        public string? ActiveIngredient { get; set; } 
        public string? Concentration { get; set; } 
        public string? Barcode { get; set; } 
        public bool RequiresPrescription { get; set; } 
        public int TotalStock { get; set; } 
        public decimal? LowestSellingPrice { get; set; } 
        public DateTime? NearestExpiryDate { get; set; } 
        public bool IsAvailable => TotalStock > 0;
    }
}
