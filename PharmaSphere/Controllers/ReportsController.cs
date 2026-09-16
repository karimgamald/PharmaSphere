using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Web.ViewModels;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> ProfitReport(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var from = fromDate?.Date
                ?? new DateTime(
                    DateTime.UtcNow.Year,
                    DateTime.UtcNow.Month,
                    1);

            var to = toDate?.Date
                ?? DateTime.UtcNow.Date;

            if (from > to)
            {
                TempData["Error"] =
                    "تاريخ البداية يجب أن يكون قبل تاريخ النهاية.";

                (from, to) = (to, from);
            }

            var orders = await _unitOfWork.Orders.GetAllAsync();

            var validOrders = orders
                .Where(o =>
                    o.OrderDate.Date >= from &&
                    o.OrderDate.Date <= to &&
                    o.OrderStatus == Domain.Enums.OrderStatus.Completed)
                .ToList();

            var orderIds = validOrders
                .Select(o => o.Id)
                .ToHashSet();

            var orderItems = await _unitOfWork.OrderItems.GetAllAsync(
                query => query
                    .Include(x => x.Medicine)
            );

            var items = orderItems
                .Where(x => orderIds.Contains(x.OrderId))
                .Select(item =>
                {
                    var salesAmount =
                        item.UnitPrice * item.Quantity;

                    var costAmount =
                        item.UnitCostPrice * item.Quantity;

                    return new ProfitReportItemViewModel
                    {
                        OrderId = item.OrderId,

                        OrderNumber =
                            validOrders
                                .First(o => o.Id == item.OrderId)
                                .OrderNumber,

                        OrderDate =
                            validOrders
                                .First(o => o.Id == item.OrderId)
                                .OrderDate,

                        MedicineName =
                            item.Medicine?.Name
                            ?? "غير معروف",

                        Quantity = item.Quantity,

                        SellingPrice = item.UnitPrice,

                        CostPrice = item.UnitCostPrice,

                        SalesAmount = salesAmount,

                        CostAmount = costAmount,

                        Profit = salesAmount - costAmount
                    };
                })
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            var totalSales =
                items.Sum(x => x.SalesAmount);

            var totalCost =
                items.Sum(x => x.CostAmount);

            var totalProfit =
                totalSales - totalCost;

            var totalOrders =
                validOrders.Count;

            var averageOrderValue =
                totalOrders > 0
                    ? totalSales / totalOrders
                    : 0;

            var profitMargin =
                totalSales > 0
                    ? totalProfit / totalSales * 100
                    : 0;

            var model = new ProfitReportViewModel
            {
                FromDate = from,
                ToDate = to,

                TotalSales = totalSales,
                TotalCost = totalCost,
                TotalProfit = totalProfit,
                ProfitMargin = profitMargin,

                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,

                Items = items
            };

            return View(model);
        }
    }
}