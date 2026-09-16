using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaSphere.Web.ViewModels
{
    public class CreateReturnViewModel
    {
        public int OrderId { get; set; }

        public int OrderItemId { get; set; }

        public int ReturnedUnits { get; set; }

        public string? Reason { get; set; }

        public List<SelectListItem> Orders { get; set; } = new();

        public List<SelectListItem> OrderItems { get; set; } = new();
    }
}
