using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PharmaSphere.Web.ViewModels
{
    public class ProfitReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalSales { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalProfit { get; set; }

        public decimal ProfitMargin { get; set; }

        public int TotalOrders { get; set; }

        public decimal AverageOrderValue { get; set; }

        public List<ProfitReportItemViewModel> Items { get; set; }
            = new();
    }
}