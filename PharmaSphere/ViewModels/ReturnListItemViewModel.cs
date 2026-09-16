namespace PharmaSphere.Web.ViewModels
{
    public class ReturnListItemViewModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string MedicineName { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        public int ReturnedUnits { get; set; }

        public decimal RefundAmount { get; set; }

        public string? Reason { get; set; }

        public DateTime ReturnedAt { get; set; } = DateTime.UtcNow;
    }
}
