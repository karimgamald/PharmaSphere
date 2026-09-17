using System.ComponentModel.DataAnnotations;

namespace PharmaSphere.Web.ViewModels
{
    public class SubstituteSearchViewModel { 
        [Required(ErrorMessage = "من فضلك اختر الدواء.")]
        [Display(Name = "الدواء")] 
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; } 
        public string? ActiveIngredient { get; set; }
        public string? Concentration { get; set; } 
        public List<SubstituteMedicineViewModel> Substitutes { get; set; } = new List<SubstituteMedicineViewModel>(); 
    }
}
