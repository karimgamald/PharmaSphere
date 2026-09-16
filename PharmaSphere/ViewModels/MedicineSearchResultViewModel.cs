namespace PharmaSphere.Web.ViewModels
{
    // نتيجة البحث السريع للأدوية في شاشة الـ POS
    public class MedicineSearchResultViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableStockUnits { get; set; }
    }
}