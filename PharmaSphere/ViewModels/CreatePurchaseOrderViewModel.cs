using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaSphere.Web.ViewModels
{
    // نموذج إنشاء فاتورة توريد جديدة
    public class CreatePurchaseOrderViewModel
    {
        [Required(ErrorMessage = "برجاء اختيار المورد")]
        [Display(Name = "المورد")]
        public int SupplierId { get; set; }

        [Display(Name = "رقم الفاتورة")]
        public string? InvoiceNumber { get; set; }

        [Required(ErrorMessage = "تاريخ الشراء مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الشراء")]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public List<PurchaseOrderItemViewModel> Items { get; set; } = new();

        // القائمة المنسدلة للموردين لاستخدامها في الواجهة
        public IEnumerable<SelectListItem>? Suppliers { get; set; }
    }
}