using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using System.Security.Claims;

namespace PharmaSphere.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // GET: /Orders
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync(
                query => query
                    .Include(o => o.Client)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Medicine)
            );

            return View(orders);
        }


        // =========================================================
        // GET: /Orders/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(
                id,
                query => query
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Medicine)

                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MedicineBatch)

                    .Include(o => o.Prescription)

                    .Include(o => o.Client)

                    .Include(o => o.Pharmacist)

                    .Include(o => o.DeliveryBoy)

                    .Include(o => o.PaymentTransaction)
            );

            if (order == null)
                return NotFound();

            return View(order);
        }


        // =========================================================
        // GET: /Orders/UpdateStatus/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);

            if (order == null)
                return NotFound();

            return View(order);
        }


        // =========================================================
        // POST: /Orders/UpdateStatus
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int orderId,
            OrderStatus status)
        {
            var order =
                await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null)
                return NotFound();

            // لا يمكن تغيير حالة الطلب إذا كان Cancelled
            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                TempData["ErrorMessage"] =
                    "لا يمكن تغيير حالة طلب تم إلغاؤه.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = orderId });
            }

            order.OrderStatus = status;

            _unitOfWork.Orders.Update(order);

            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] =
                $"تم تحديث حالة الطلب #{order.OrderNumber} بنجاح.";

            return RedirectToAction(
                nameof(Details),
                new { id = orderId });
        }


        // =========================================================
        // GET: /Orders/Cancel/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(
                id,
                query => query
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Medicine)

                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MedicineBatch)

                    .Include(o => o.Client)
            );

            if (order == null)
                return NotFound();

            // يمكن إلغاء الطلب فقط إذا كان Pending
            if (order.OrderStatus != OrderStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "لا يمكن إلغاء هذا الطلب لأن حالته الحالية لا تسمح بالإلغاء.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            return View(order);
        }


        // =========================================================
        // POST: /Orders/CancelConfirmed/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(
                id,
                query => query
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Medicine)

                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.MedicineBatch)
            );

            if (order == null)
                return NotFound();


            // =====================================================
            // منع إلغاء الطلب أكثر من مرة
            // =====================================================
            if (order.OrderStatus != OrderStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "لا يمكن إلغاء الطلب. يجب أن تكون حالة الطلب Pending.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =====================================================
            // Current User
            // =====================================================
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);


            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // =================================================
                // إرجاع كل Item إلى الـ Stock
                // =================================================
                foreach (var item in order.OrderItems)
                {
                    var batch = item.MedicineBatch;

                    if (batch == null)
                        continue;


                    // ---------------------------------------------
                    // حماية من الكمية غير الصحيحة
                    // ---------------------------------------------
                    if (item.Quantity <= 0)
                        continue;


                    // ---------------------------------------------
                    // إعادة الكمية إلى المخزون
                    // ---------------------------------------------
                    batch.RemainingUnits += item.Quantity;

                    _unitOfWork.MedicineBatches.Update(batch);


                    // ---------------------------------------------
                    // تسجيل Stock Transaction
                    // ---------------------------------------------
                    var stockTransaction = new StockTransaction
                    {
                        MedicineBatchId = batch.Id,

                        Type = StockTransactionType.Return,

                        Quantity = item.Quantity,

                        BalanceAfterTransaction =
                            batch.RemainingUnits,

                        Reference =
                            order.OrderNumber,

                        Notes =
                            $"إلغاء الطلب #{order.OrderNumber} وإرجاع {item.Quantity} وحدة من المخزون.",

                        CreatedById = currentUserId,

                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.StockTransactions
                        .AddAsync(stockTransaction);
                }


                // =================================================
                // تغيير حالة الطلب
                // =================================================
                order.OrderStatus =
                    OrderStatus.Cancelled;

                _unitOfWork.Orders.Update(order);


                // =================================================
                // Save Changes
                // =================================================
                await _unitOfWork.CompleteAsync();


                // =================================================
                // Commit Transaction
                // =================================================
                await _unitOfWork.CommitTransactionAsync();


                TempData["SuccessMessage"] =
                    $"تم إلغاء الطلب #{order.OrderNumber} وإرجاع الكميات إلى المخزون بنجاح.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["ErrorMessage"] =
                    "حدث خطأ أثناء إلغاء الطلب. لم يتم تنفيذ عملية الإلغاء.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }
    }
}