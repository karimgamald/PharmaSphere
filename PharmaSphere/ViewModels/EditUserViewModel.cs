using PharmaSphere.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PharmaSphere.Web.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الكامل مطلوب.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "الاسم يجب أن يكون بين 3 و100 حرف.")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح.")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح.")]
        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "يجب اختيار الدور.")]
        [Display(Name = "الدور")]
        public UserRole Role { get; set; }
    }
}
