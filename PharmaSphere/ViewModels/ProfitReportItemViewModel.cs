using System;

namespace PharmaSphere.Web.ViewModels
{
    public class ProfitReportItemViewModel
    {
        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public string MedicineName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalesAmount { get; set; }

        public decimal CostAmount { get; set; }

        public decimal Profit { get; set; }
    }
}