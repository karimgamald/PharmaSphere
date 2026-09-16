using PharmaSphere.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PharmaSphere.Web.ViewModels
{
    public class CreateUserViewModel
    {
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

        [StringLength(500, ErrorMessage = "العنوان طويل جدًا.")]
        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "يجب اختيار الدور.")]
        [Display(Name = "الدور")]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        [DataType(DataType.Password)]
        [MinLength(6,
            ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل.")]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب.")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "كلمتا المرور غير متطابقتين.")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
