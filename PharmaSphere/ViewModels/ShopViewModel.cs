using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Web.ViewModels
{
    public class ShopViewModel
    {
        public IEnumerable<Medicine> Medicines { get; set; } = new List<Medicine>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public int? CategoryId { get; set; }

        public string? Search { get; set; }
    }
}