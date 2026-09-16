using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using PharmaSphere.Web.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class PurchaseOrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PurchaseOrdersController> _logger;

        public PurchaseOrdersController(
            IUnitOfWork unitOfWork,
            ILogger<PurchaseOrdersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // =========================================================
        // GET: /PurchaseOrders/Create
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();

            ViewBag.Medicines = medicines
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToList();

            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();

            var model = new CreatePurchaseOrderViewModel
            {
                Suppliers = suppliers
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        // =========================================================
        // POST: /PurchaseOrders/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] CreatePurchaseOrderViewModel model)
        {
            // -----------------------------------------------------
            // 1. Validate request
            // -----------------------------------------------------

            if (model == null)
            {
                return Json(new
                {
                    success = false,
                    message = "بيانات الفاتورة غير صحيحة."
                });
            }

            if (model.Items == null || !model.Items.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "الفاتورة لا تحتوي على أي بنود دواء."
                });
            }

            if (model.SupplierId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "يرجى اختيار المورد."
                });
            }

            if (string.IsNullOrWhiteSpace(model.InvoiceNumber))
            {
                return Json(new
                {
                    success = false,
                    message = "رقم الفاتورة مطلوب."
                });
            }

            if (model.PurchaseDate == default)
            {
                return Json(new
                {
                    success = false,
                    message = "تاريخ التوريد مطلوب."
                });
            }

            if (model.PurchaseDate > DateTime.UtcNow)
            {
                return Json(new
                {
                    success = false,
                    message = "تاريخ التوريد لا يمكن أن يكون في المستقبل."
                });
            }


            // -----------------------------------------------------
            // 2. Get current user
            // -----------------------------------------------------

            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }


            // -----------------------------------------------------
            // 3. Validate supplier before transaction
            // -----------------------------------------------------

            var supplier =
                await _unitOfWork.Suppliers
                    .GetByIdAsync(model.SupplierId);

            if (supplier == null)
            {
                return Json(new
                {
                    success = false,
                    message = "المورد غير موجود."
                });
            }

            if (!supplier.IsActive)
            {
                return Json(new
                {
                    success = false,
                    message = "المورد غير نشط ولا يمكن تسجيل فاتورة توريد منه."
                });
            }


            // -----------------------------------------------------
            // 4. Validate all items BEFORE changing database
            // -----------------------------------------------------

            decimal totalAmount = 0;

            foreach (var item in model.Items)
            {
                // Quantity
                if (item.QuantityUnits <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "كمية الدواء يجب أن تكون أكبر من صفر."
                    });
                }

                // Purchase price
                if (item.PurchasePrice < 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "سعر الشراء لا يمكن أن يكون سالباً."
                    });
                }

                // Selling price
                if (item.SellingPrice < 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "سعر البيع لا يمكن أن يكون سالباً."
                    });
                }

                // Selling price must be >= purchase price
                if (item.SellingPrice < item.PurchasePrice)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "سعر البيع لا يمكن أن يكون أقل من سعر الشراء."
                    });
                }

                // Batch number
                if (string.IsNullOrWhiteSpace(item.BatchNumber))
                {
                    return Json(new
                    {
                        success = false,
                        message = "رقم التشغيلة مطلوب."
                    });
                }

                var batchNumber =
                    item.BatchNumber.Trim();

                // Batch number length
                if (batchNumber.Length > 50)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "رقم التشغيلة لا يمكن أن يتجاوز 50 حرفاً."
                    });
                }

                // Expiry date
                if (item.ExpiryDate.Date <= DateTime.UtcNow.Date)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            $"تاريخ انتهاء التشغيلة {batchNumber} يجب أن يكون في المستقبل."
                    });
                }

                // Medicine
                var medicine =
                    await _unitOfWork.Medicines
                        .GetByIdAsync(item.MedicineId);

                if (medicine == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "أحد الأدوية غير موجود."
                    });
                }

                if (!medicine.IsActive)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            $"الدواء {medicine.Name} غير نشط ولا يمكن توريده."
                    });
                }

                // -------------------------------------------------
                // Prevent duplicate batch number
                // for the same medicine
                // -------------------------------------------------

                var existingBatches =
                    await _unitOfWork.MedicineBatches.GetAllAsync();

                var duplicateBatch =
                    existingBatches.Any(b =>
                        b.MedicineId == item.MedicineId &&
                        b.BatchNumber.Trim()
                            .Equals(
                                batchNumber,
                                StringComparison.OrdinalIgnoreCase));

                if (duplicateBatch)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            $"رقم التشغيلة {batchNumber} موجود بالفعل للدواء {medicine.Name}."
                    });
                }

                // Calculate invoice total
                totalAmount +=
                    item.PurchasePrice *
                    item.QuantityUnits;
            }


            // -----------------------------------------------------
            // 5. Begin transaction
            // -----------------------------------------------------

            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // -------------------------------------------------
                // 6. Create Purchase Order
                // -------------------------------------------------

                var purchaseOrder = new PurchaseOrder
                {
                    SupplierId = model.SupplierId,

                    InvoiceNumber =
                        model.InvoiceNumber.Trim(),

                    OrderDate =
                        model.PurchaseDate,

                    TotalAmount =
                        totalAmount
                };

                await _unitOfWork.PurchaseOrders
                    .AddAsync(purchaseOrder);


                // -------------------------------------------------
                // 7. Create batches + purchase items
                //    + stock transactions
                // -------------------------------------------------

                foreach (var item in model.Items)
                {
                    var batch = new MedicineBatch
                    {
                        MedicineId =
                            item.MedicineId,

                        BatchNumber =
                            item.BatchNumber.Trim(),

                        ExpiryDate =
                            item.ExpiryDate,

                        PurchasePrice =
                            item.PurchasePrice,

                        SellingPrice =
                            item.SellingPrice,

                        OriginalUnits =
                            item.QuantityUnits,

                        RemainingUnits =
                            item.QuantityUnits,

                        PurchaseOrder =
                            purchaseOrder
                    };

                    await _unitOfWork.MedicineBatches
                        .AddAsync(batch);


                    // -------------------------------------------------
                    // Purchase Order Item
                    // -------------------------------------------------

                    var purchaseItem =
                        new PurchaseOrderItem
                        {
                            PurchaseOrder =
                                purchaseOrder,

                            MedicineId =
                                item.MedicineId,

                            BatchNumber =
                                item.BatchNumber.Trim(),

                            ExpiryDate =
                                item.ExpiryDate,

                            Quantity =
                                item.QuantityUnits,

                            PurchasePrice =
                                item.PurchasePrice,

                            SellingPrice =
                                item.SellingPrice,

                            TotalPrice =
                                item.PurchasePrice *
                                item.QuantityUnits
                        };

                    await _unitOfWork.PurchaseOrderItems
                        .AddAsync(purchaseItem);


                    // -------------------------------------------------
                    // Stock Transaction
                    // -------------------------------------------------

                    var stockTransaction =
                        new StockTransaction
                        {
                            MedicineBatch =
                                batch,

                            Type =
                                StockTransactionType.Purchase,

                            Quantity =
                                item.QuantityUnits,

                            BalanceAfterTransaction =
                                item.QuantityUnits,

                            Reference =
                                purchaseOrder.InvoiceNumber,

                            Notes =
                                $"Purchase from supplier #{supplier.Id}",

                            CreatedById =
                                currentUserId,

                            CreatedAt =
                                DateTime.UtcNow
                        };

                    await _unitOfWork.StockTransactions
                        .AddAsync(stockTransaction);
                }


                // -------------------------------------------------
                // 8. Save everything ONCE
                // -------------------------------------------------

                await _unitOfWork.CompleteAsync();


                // -------------------------------------------------
                // 9. Commit transaction
                // -------------------------------------------------

                await _unitOfWork.CommitTransactionAsync();


                // -------------------------------------------------
                // 10. Success
                // -------------------------------------------------

                return Json(new
                {
                    success = true,

                    purchaseOrderId =
                        purchaseOrder.Id,

                    invoiceNumber =
                        purchaseOrder.InvoiceNumber,

                    totalAmount =
                        purchaseOrder.TotalAmount,

                    message =
                        "تم تسجيل فاتورة التوريد وتحديث المخزون بنجاح."
                });
            }
            catch (Exception ex)
            {
                // ---------------------------------------------
                // Rollback database transaction
                // ---------------------------------------------

                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error while creating purchase order. SupplierId: {SupplierId}",
                    model.SupplierId);

                return Json(new
                {
                    success = false,
                    message =
                        "حدث خطأ أثناء حفظ فاتورة التوريد. يرجى المحاولة مرة أخرى."
                });
            }
        }
    }
}
