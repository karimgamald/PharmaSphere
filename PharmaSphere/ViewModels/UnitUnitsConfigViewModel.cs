using System.ComponentModel.DataAnnotations;

namespace PharmaSphere.Web.ViewModels
{
    public class UnitUnitsConfigViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ScientificName { get; set; }

        public string? Barcode { get; set; }

        [Range(1, 1000)]
        [Display(Name = "عدد الوحدات داخل العبوة")]
        public int UnitsPerBox { get; set; } = 1;
    }
}
