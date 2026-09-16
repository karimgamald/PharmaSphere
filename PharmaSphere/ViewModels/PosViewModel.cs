using PharmaSphere.Application.DTOs;

namespace PharmaSphere.Web.ViewModels
{
    public class PosViewModel
    {
        public List<PosMedicineSearchDto> InitialMedicines { get; set; } = new();
    }
}
