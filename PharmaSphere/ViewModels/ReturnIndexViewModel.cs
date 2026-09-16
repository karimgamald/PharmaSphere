namespace PharmaSphere.Web.ViewModels
{
    public class ReturnIndexViewModel
    {
        public List<ReturnListItemViewModel> Returns { get; set; } = new();

        public decimal TotalRefunds { get; set; }

        public int TotalReturns { get; set; }
    }
}
