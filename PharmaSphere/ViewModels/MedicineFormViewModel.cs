using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaSphere.Web.ViewModels
{
    public class MedicineFormViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ScientificName { get; set; } = string.Empty;

        public string ActiveIngredient { get; set; } = string.Empty;

        public string ActiveIngredientConcentration { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        public int UnitsPerBox { get; set; } = 1;

        public bool RequiresPrescription { get; set; }

        public string? ImagePath { get; set; }

        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}