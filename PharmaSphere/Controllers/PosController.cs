using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using PharmaSphere.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class PosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        // =========================================================
        // GET: /Pos
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new PosCheckoutViewModel();

            // Load medicines and available batches
            var medicines = await _unitOfWork.Medicines.GetAllAsync();

            var batches = await _unitOfWork.MedicineBatches.GetAllAsync();

            var now = DateTime.UtcNow;

            // Get medicines that have available valid stock
            var availableMedicines = medicines
                .Where(m => m.IsActive)
                .Select(m =>
                {
                    var validBatches = batches
                        .Where(b =>
                            b.MedicineId == m.Id &&
                            b.ExpiryDate > now &&
                            b.RemainingUnits > 0)
                        .OrderBy(b => b.ExpiryDate)
                        .ToList();

                    var firstBatch = validBatches.FirstOrDefault();

                    return new MedicineSearchResultViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        ScientificName = m.ScientificName ?? string.Empty,
                        Barcode = m.Barcode ?? string.Empty,

                        // Price is taken from FEFO batch
                        Price = firstBatch?.SellingPrice ?? 0,

                        AvailableStockUnits =
                            validBatches.Sum(b => b.RemainingUnits)
                    };
                })
                .Where(m => m.AvailableStockUnits > 0)
                .OrderBy(m => m.Name)
                .ToList();

            model.Medicines = availableMedicines;

            return View(model);
        }


        // =========================================================
        // GET: /Pos/SearchMedicines?query=panadol
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> SearchMedicines(string? query)
        {
            var medicines =
                await _unitOfWork.Medicines.GetAllAsync();

            var batches =
                await _unitOfWork.MedicineBatches.GetAllAsync();

            var now = DateTime.UtcNow;

            query = query?.Trim();


            // -----------------------------------------------------
            // Filter medicines
            // If query is empty => return available medicines
            // -----------------------------------------------------

            var filteredMedicines = medicines
                .Where(m => m.IsActive)
                .Where(m =>
                    string.IsNullOrWhiteSpace(query)

                    ||

                    m.Name.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    (!string.IsNullOrWhiteSpace(m.ScientificName)
                     &&
                     m.ScientificName.Contains(
                         query,
                         StringComparison.OrdinalIgnoreCase))

                    ||

                    (!string.IsNullOrWhiteSpace(m.Barcode)
                     &&
                     m.Barcode.Equals(
                         query,
                         StringComparison.OrdinalIgnoreCase))
                );


            // -----------------------------------------------------
            // Build POS result
            // -----------------------------------------------------

            var searchResults = filteredMedicines
                .Select(m =>
                {
                    var validBatches = batches
                        .Where(b =>
                            b.MedicineId == m.Id &&
                            b.ExpiryDate > now &&
                            b.RemainingUnits > 0)
                        .OrderBy(b => b.ExpiryDate)
                        .ToList();

                    var firstBatch =
                        validBatches.FirstOrDefault();

                    return new MedicineSearchResultViewModel
                    {
                        Id = m.Id,

                        Name = m.Name,

                        ScientificName =
                            m.ScientificName ?? string.Empty,

                        Barcode =
                            m.Barcode ?? string.Empty,

                        // IMPORTANT:
                        // Price belongs to MedicineBatch
                        // and comes from the FEFO batch.
                        Price =
                            firstBatch?.SellingPrice ?? 0,

                        AvailableStockUnits =
                            validBatches.Sum(
                                b => b.RemainingUnits)
                    };
                })
                .Where(m => m.AvailableStockUnits > 0)
                .OrderBy(m => m.Name)
                .Take(30)
                .ToList();

            return Json(searchResults);
        }


        // =========================================================
        // POST: /Pos/Checkout
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            [FromBody] PosCheckoutViewModel model)
        {
            if (model == null ||
                model.Items == null ||
                !model.Items.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "سلة المشتريات فارغة."
                });
            }


            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();


            try
            {
                decimal subTotal = 0;

                var orderItems =
                    new List<OrderItem>();

                var stockTransactions =
                    new List<StockTransaction>();


                // =================================================
                // Group duplicated medicine items
                // =================================================

                var requestedItems = model.Items
                    .GroupBy(x => x.MedicineId)
                    .Select(g => new
                    {
                        MedicineId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();


                // =================================================
                // Process each medicine using FEFO
                // =================================================

                foreach (var item in requestedItems)
                {
                    if (item.Quantity <= 0)
                    {
                        await _unitOfWork
                            .RollbackTransactionAsync();

                        return Json(new
                        {
                            success = false,
                            message =
                                "الكمية يجب أن تكون أكبر من صفر."
                        });
                    }


                    // -------------------------------------------------
                    // Load medicine
                    // -------------------------------------------------

                    var medicine =
                        await _unitOfWork.Medicines
                            .GetByIdAsync(item.MedicineId);


                    if (medicine == null ||
                        !medicine.IsActive)
                    {
                        await _unitOfWork
                            .RollbackTransactionAsync();

                        return Json(new
                        {
                            success = false,
                            message =
                                "الدواء غير موجود أو غير نشط."
                        });
                    }


                    // -------------------------------------------------
                    // Get valid batches
                    // FEFO = First Expired, First Out
                    // -------------------------------------------------

                    var batches =
                        (await _unitOfWork.MedicineBatches
                            .GetAllAsync())
                        .Where(b =>
                            b.MedicineId == medicine.Id &&
                            b.ExpiryDate > DateTime.UtcNow &&
                            b.RemainingUnits > 0)
                        .OrderBy(b => b.ExpiryDate)
                        .ToList();


                    int availableStock =
                        batches.Sum(
                            b => b.RemainingUnits);


                    // -------------------------------------------------
                    // Check stock
                    // -------------------------------------------------

                    if (availableStock < item.Quantity)
                    {
                        await _unitOfWork
                            .RollbackTransactionAsync();

                        return Json(new
                        {
                            success = false,

                            message =
                                $"المخزون غير كافٍ للدواء ({medicine.Name}). " +
                                $"المتاح: {availableStock} وحدة."
                        });
                    }


                    int remainingQuantity =
                        item.Quantity;


                    // =================================================
                    // Deduct from batches using FEFO
                    // =================================================

                    foreach (var batch in batches)
                    {
                        if (remainingQuantity <= 0)
                            break;


                        int soldUnits =
                            Math.Min(
                                batch.RemainingUnits,
                                remainingQuantity);


                        // ---------------------------------------------
                        // Price comes from DATABASE
                        // ---------------------------------------------

                        decimal unitSellingPrice =
                            batch.SellingPrice;

                        decimal unitCostPrice =
                            batch.PurchasePrice;


                        decimal itemTotal =
                            unitSellingPrice *
                            soldUnits;


                        // ---------------------------------------------
                        // Update stock
                        // ---------------------------------------------

                        batch.RemainingUnits -= soldUnits;

                        _unitOfWork.MedicineBatches
                            .Update(batch);


                        // ---------------------------------------------
                        // Create OrderItem
                        // One OrderItem per Batch
                        // ---------------------------------------------

                        orderItems.Add(
                            new OrderItem
                            {
                                MedicineId =
                                    medicine.Id,

                                MedicineBatchId =
                                    batch.Id,

                                Quantity =
                                    soldUnits,

                                UnitPrice =
                                    unitSellingPrice,

                                UnitCostPrice =
                                    unitCostPrice,

                                TotalPrice =
                                    itemTotal
                            });


                        subTotal += itemTotal;


                        // ---------------------------------------------
                        // Stock Transaction
                        // Sale = negative quantity
                        // ---------------------------------------------

                        stockTransactions.Add(
                            new StockTransaction
                            {
                                MedicineBatchId =
                                    batch.Id,

                                Type =
                                    StockTransactionType.Sale,

                                Quantity =
                                    -soldUnits,

                                BalanceAfterTransaction =
                                    batch.RemainingUnits,

                                Reference =
                                    "POS",

                                Notes =
                                    $"POS Sale - {medicine.Name}",

                                CreatedAt =
                                    DateTime.UtcNow
                            });


                        remainingQuantity -=
                            soldUnits;
                    }
                }


                // =================================================
                // Discount
                // =================================================

                decimal discount =
                    model.DiscountAmount < 0
                        ? 0
                        : model.DiscountAmount;


                if (discount > subTotal)
                    discount = subTotal;


                decimal calculatedTotal =
                    subTotal - discount;


                // =================================================
                // Create Order
                // =================================================

                var order = new Order
                {
                    OrderNumber =
                        GenerateOrderNumber(),

                    OrderType =
                        OrderType.POS_InStore,

                    OrderStatus =
                        OrderStatus.Completed,

                    // POS is CASH and payment is completed
                    PaymentStatus =
                        PaymentStatus.Paid,

                    PaymentMethod =
                        PaymentMethod.Cash,

                    SubTotal =
                        subTotal,

                    DiscountAmount =
                        discount,

                    DeliveryFee =
                        0,

                    TotalAmount =
                        calculatedTotal,

                    OrderDate =
                        DateTime.UtcNow,

                    ClientId =
                        null,

                    PharmacistId =
                        null,

                    DeliveryBoyId =
                        null,

                    OrderItems =
                        orderItems
                };


                await _unitOfWork.Orders
                    .AddAsync(order);


                // =================================================
                // Add Stock Transactions
                // =================================================

                foreach (var stockTransaction
                         in stockTransactions)
                {
                    await _unitOfWork.StockTransactions
                        .AddAsync(stockTransaction);
                }


                // =================================================
                // Save
                // =================================================

                await _unitOfWork.CompleteAsync();


                // Commit ONLY
                // CommitTransactionAsync should not call SaveChanges
                await _unitOfWork
                    .CommitTransactionAsync();


                // =================================================
                // Success
                // =================================================

                return Json(new
                {
                    success = true,

                    orderId =
                        order.Id,

                    orderNumber =
                        order.OrderNumber,

                    subTotal =
                        subTotal,

                    discount =
                        discount,

                    total =
                        calculatedTotal,

                    message =
                        "تمت عملية البيع بنجاح!"
                });
            }
            catch
            {
                await _unitOfWork
                    .RollbackTransactionAsync();

                return Json(new
                {
                    success = false,

                    message =
                        "حدث خطأ أثناء معالجة عملية البيع."
                });
            }
        }


        // =========================================================
        // Generate Order Number
        // =========================================================

        private static string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyMMddHHmm");
            var random = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpperInvariant();

            return $"ON-{timestamp}-{random}";
        }
    }
}