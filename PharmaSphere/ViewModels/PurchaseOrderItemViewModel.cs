using System;
using System.ComponentModel.DataAnnotations;

namespace PharmaSphere.Web.ViewModels
{
    public class PurchaseOrderItemViewModel
    {
        [Required(ErrorMessage = "يرجى اختيار الدواء")]
        public int MedicineId { get; set; }

        [Required(ErrorMessage = "رقم التشغيلة (الباتش) مطلوب")]
        public string BatchNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الانتهاء مطلوب")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Required(ErrorMessage = "سعر الشراء مطلوب")]
        [Range(0.01, 100000, ErrorMessage = "سعر الشراء يجب أن يكون أكبر من 0")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "سعر البيع مطلوب")]
        [Range(0.01, 100000, ErrorMessage = "سعر البيع يجب أن يكون أكبر من 0")]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "الكمية الواردة بالوحدة مطلوبة")]
        [Range(1, 100000, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل")]
        public int QuantityUnits { get; set; }

        public decimal TotalCost => PurchasePrice * QuantityUnits;
    }
}