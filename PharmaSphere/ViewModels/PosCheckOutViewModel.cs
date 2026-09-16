using System.Collections.Generic;

namespace PharmaSphere.Web.ViewModels
{
    public class PosCheckoutViewModel
    {
        public List<PosItemViewModel> Items { get; set; } = new();
        public decimal DiscountAmount { get; set; } = 0.00m;
        public List<MedicineSearchResultViewModel> Medicines { get; set; } = new();
    }

    public class PosItemViewModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}