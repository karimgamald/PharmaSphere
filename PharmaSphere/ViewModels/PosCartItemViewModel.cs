namespace PharmaSphere.Web.ViewModels
{
    public class PosCartItemViewModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } // بالوحدات / الشرائط
        public decimal Total => UnitPrice * Quantity;
    }
}
