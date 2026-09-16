using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaSphere.Web.ViewModels
{
    public class BatchFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الدواء.")]
        [Display(Name = "الدواء")]
        public int MedicineId { get; set; }

        [Required(ErrorMessage = "يرجى إدخال رقم التشغيلة.")]
        [StringLength(50, ErrorMessage = "رقم التشغيلة يجب ألا يتجاوز 50 حرفًا.")]
        [Display(Name = "رقم التشغيلة")]
        public string BatchNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى تحديد تاريخ انتهاء الصلاحية.")]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ انتهاء الصلاحية")]
        public DateTime ExpiryDate { get; set; } = DateTime.Today.AddYears(1);

        [Required(ErrorMessage = "يرجى إدخال الكمية.")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر.")]
        [Display(Name = "الكمية / عدد الوحدات")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "يرجى إدخال سعر الشراء.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الشراء يجب أن يكون أكبر من صفر.")]
        [DataType(DataType.Currency)]
        [Display(Name = "سعر الشراء")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "يرجى إدخال سعر البيع.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من صفر.")]
        [DataType(DataType.Currency)]
        [Display(Name = "سعر البيع")]
        public decimal SellingPrice { get; set; }

        public int? PurchaseOrderId { get; set; }

        // Dropdown options for views
        public IEnumerable<SelectListItem> Medicines { get; set; } = new List<SelectListItem>();
    }
}