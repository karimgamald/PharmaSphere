namespace PharmaSphere.Web.ViewModels
{
    public class CartItemViewModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public bool RequiresPrescription { get; set; }
        public string? ImagePath { get; set; }
    }
}