using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PharmaSphere.Web.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "يرجى إدخال عنوان التوصيل")]
        [Display(Name = "عنوان التوصيل")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى إدخال رقم الهاتف للتواصل")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "طريقة الدفع")]
        public string PaymentMethod { get; set; } = "COD"; // Cash On Delivery

        public string? PrescriptionImagePath { get; set; }

        [Display(Name = "ملف الروشتة الطبية")]
        public IFormFile? PrescriptionFile { get; set; }
    }
}