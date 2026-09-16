using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using PharmaSphere.Web.ViewModels;
using System.Security.Claims;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class ReturnsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReturnsController> _logger;

        public ReturnsController(
            IUnitOfWork unitOfWork,
            ILogger<ReturnsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var salesReturns = await _unitOfWork.SalesReturns.GetAllAsync(
                query => query
                    .Include(x => x.Order)
                    .Include(x => x.MedicineBatch)
                    .ThenInclude(x => x.Medicine)
            );

            var items = salesReturns
                .OrderByDescending(x => x.ReturnedAt)
                .Select(x => new ReturnListItemViewModel
                {
                    Id = x.Id,

                    OrderNumber =
                        x.Order?.OrderNumber ?? "غير معروف",

                    MedicineName =
                        x.MedicineBatch?.Medicine?.Name
                        ?? "غير معروف",

                    BatchNumber =
                        x.MedicineBatch?.BatchNumber
                        ?? "-",

                    ReturnedUnits = x.ReturnedUnits,

                    RefundAmount = x.RefundAmount,

                    Reason = x.Reason,

                    ReturnedAt = x.ReturnedAt
                })
                .ToList();

            var model = new ReturnIndexViewModel
            {
                Returns = items,

                TotalReturns = items.Count,

                TotalRefunds = items.Sum(x => x.RefundAmount)
            };

            return View(model);
        }


        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();

            var completedOrders = orders
                .Where(x => x.OrderStatus == OrderStatus.Completed)
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            var model = new CreateReturnViewModel
            {
                Orders = completedOrders
                    .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = $"{x.OrderNumber} - {x.OrderDate:dd/MM/yyyy}"
                    })
                    .ToList()
            };

            return View(model);
        }


        // ============================================================
        // GET ORDER ITEMS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var orderItems = await _unitOfWork.OrderItems.GetAllAsync(
                query => query
                    .Include(x => x.Medicine)
                    .Include(x => x.MedicineBatch)
            );

            var items = orderItems
                .Where(x => x.OrderId == orderId)
                .Select(x => new
                {
                    id = x.Id,

                    medicineName =
                        x.Medicine?.Name ?? "غير معروف",

                    batchNumber =
                        x.MedicineBatch?.BatchNumber ?? "-",

                    quantity = x.Quantity,

                    unitPrice = x.UnitPrice,

                    totalPrice = x.TotalPrice
                })
                .ToList();

            return Json(items);
        }


        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateReturnViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            if (model.OrderId <= 0)
            {
                ModelState.AddModelError(
                    "OrderId",
                    "يجب اختيار الطلب.");
            }

            if (model.OrderItemId <= 0)
            {
                ModelState.AddModelError(
                    "OrderItemId",
                    "يجب اختيار الدواء.");
            }

            if (model.ReturnedUnits <= 0)
            {
                ModelState.AddModelError(
                    "ReturnedUnits",
                    "كمية المرتجع يجب أن تكون أكبر من صفر.");
            }

            if (!ModelState.IsValid)
            {
                await LoadOrders(model);

                return View(model);
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(
                model.OrderId);

            if (order == null)
            {
                return NotFound("الطلب غير موجود.");
            }

            if (order.OrderStatus != OrderStatus.Completed)
            {
                TempData["Error"] =
                    "لا يمكن إرجاع منتجات من طلب غير مكتمل.";

                return RedirectToAction(nameof(Index));
            }

            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(
                model.OrderItemId,
                query => query
                    .Include(x => x.Medicine)
                    .Include(x => x.MedicineBatch)
            );

            if (orderItem == null ||
                orderItem.OrderId != model.OrderId)
            {
                TempData["Error"] =
                    "عنصر الطلب غير موجود.";

                return RedirectToAction(nameof(Index));
            }

            if (model.ReturnedUnits > orderItem.Quantity)
            {
                ModelState.AddModelError(
                    "ReturnedUnits",
                    "كمية المرتجع أكبر من الكمية المباعة.");

                await LoadOrders(model);

                return View(model);
            }

            var batch = orderItem.MedicineBatch;

            if (batch == null)
            {
                TempData["Error"] =
                    "دفعة الدواء المرتبطة بالطلب غير موجودة.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Prevent returning the same quantity multiple times
            // --------------------------------------------------------

            var existingReturns =
                await _unitOfWork.SalesReturns.GetAllAsync();

            var alreadyReturned = existingReturns
                .Where(x =>
                    x.OrderId == model.OrderId &&
                    x.MedicineBatchId == batch.Id)
                .Sum(x => x.ReturnedUnits);

            var remainingReturnable =
                orderItem.Quantity - alreadyReturned;

            if (model.ReturnedUnits > remainingReturnable)
            {
                ModelState.AddModelError(
                    "ReturnedUnits",
                    $"الكمية المتاحة للإرجاع هي {remainingReturnable} فقط.");

                await LoadOrders(model);

                return View(model);
            }

            var refundAmount =
                orderItem.UnitPrice * model.ReturnedUnits;

            var currentUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // ----------------------------------------------------
                // Add stock back
                // ----------------------------------------------------

                batch.RemainingUnits += model.ReturnedUnits;

                // ----------------------------------------------------
                // Create SalesReturn
                // ----------------------------------------------------

                var salesReturn = new SalesReturn
                {
                    OrderId = model.OrderId,
                    OrderItemId = orderItem.Id,

                    MedicineBatchId = batch.Id,

                    ReturnedUnits = model.ReturnedUnits,

                    RefundAmount = refundAmount,

                    Reason = model.Reason?.Trim(),

                    ReturnedAt = DateTime.UtcNow,

                    // If your SalesReturn entity has OrderItemId,
                    // add this:
                    //
                    // OrderItemId = model.OrderItemId
                };

                await _unitOfWork.SalesReturns.AddAsync(
                    salesReturn);

                // ----------------------------------------------------
                // Stock Transaction
                // ----------------------------------------------------

                var stockTransaction = new StockTransaction
                {
                    MedicineBatchId = batch.Id,

                    Type = StockTransactionType.Return,

                    Quantity = model.ReturnedUnits,

                    BalanceAfterTransaction =
                        batch.RemainingUnits,

                    Reference =
                        order.OrderNumber,

                    Notes =
                        $"مرتجع من الطلب {order.OrderNumber}",

                    CreatedById = currentUserId,

                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.StockTransactions.AddAsync(
                    stockTransaction);

                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();

                TempData["Success"] =
                    "تم تسجيل المرتجع وإضافة الكمية إلى المخزون بنجاح.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error creating sales return for OrderId {OrderId}",
                    model.OrderId);

                TempData["Error"] =
                    "حدث خطأ أثناء تسجيل المرتجع.";

                return RedirectToAction(nameof(Index));
            }
        }


        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return NotFound();

            var salesReturn =
                await _unitOfWork.SalesReturns.GetByIdAsync(
                    id,
                    query => query
                        .Include(x => x.Order)
                        .Include(x => x.MedicineBatch)
                        .ThenInclude(x => x.Medicine)
                );

            if (salesReturn == null)
                return NotFound();

            return View(salesReturn);
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private async Task LoadOrders(
            CreateReturnViewModel model)
        {
            var orders =
                await _unitOfWork.Orders.GetAllAsync();

            model.Orders = orders
                .Where(x =>
                    x.OrderStatus == OrderStatus.Completed)
                .OrderByDescending(x => x.OrderDate)
                .Select(x =>
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = x.Id.ToString(),

                        Text =
                            $"{x.OrderNumber} - {x.OrderDate:dd/MM/yyyy}",

                        Selected =
                            x.Id == model.OrderId
                    })
                .ToList();
        }
    }
}