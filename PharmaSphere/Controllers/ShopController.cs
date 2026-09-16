using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using PharmaSphere.Web.Helpers;
using PharmaSphere.Web.ViewModels;

namespace PharmaSphere.Web.Controllers
{
    //[Authorize]
    public class ShopController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private const string CartSessionKey =
            "PharmaSphere_CartSession";

        private const decimal DeliveryFeeAmount = 15.00m;

        public ShopController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // GET: /Shop
        // =========================================================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(
    int? categoryId,
    string? search)
        {
            var categories = await _unitOfWork.Categories
                .GetAllAsync();

            var medicines = await _unitOfWork.Medicines
                .GetAllAsync();

            var allBatches = await _unitOfWork.MedicineBatches
                .GetAllAsync();

            medicines = medicines
                .Where(m => m.IsActive);

            if (categoryId.HasValue)
            {
                medicines = medicines
                    .Where(m => m.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                medicines = medicines.Where(m =>
                    m.Name.Contains(search) ||
                    (m.ScientificName != null &&
                     m.ScientificName.Contains(search)) ||
                    (m.ActiveIngredient != null &&
                     m.ActiveIngredient.Contains(search)));
            }

            foreach (var medicine in medicines)
            {
                medicine.Batches = allBatches
                    .Where(b => b.MedicineId == medicine.Id)
                    .OrderBy(b => b.ExpiryDate)
                    .ToList();
            }

            var model = new ShopViewModel
            {
                Medicines = medicines.ToList(),

                Categories = categories
                    .Where(c => c.IsActive)
                    .ToList(),

                CategoryId = categoryId,

                Search = search
            };

            return View(model);
        }


        // =========================================================
        // GET: /Shop/Cart
        // =========================================================
        [HttpGet]
        public IActionResult Cart()
        {
            var cartItems =
                GetCart();

            var cartViewModel =
                new CartViewModel
                {
                    CartItems =
                        cartItems,

                    DeliveryFee =
                        cartItems.Any()
                            ? DeliveryFeeAmount
                            : 0.00m
                };

            return View(cartViewModel);
        }


        // =========================================================
        // POST: /Shop/AddToCart
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            int medicineId,
            int quantity = 1)
        {
            // -----------------------------------------------------
            // Validate quantity
            // -----------------------------------------------------
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] =
                    "الكمية يجب أن تكون أكبر من صفر.";

                return RedirectToAction(
                    nameof(Index));
            }

            // -----------------------------------------------------
            // Get medicine
            // -----------------------------------------------------
            var medicine =
                await _unitOfWork
                    .Medicines
                    .GetByIdAsync(medicineId);

            if (medicine == null ||
                !medicine.IsActive)
            {
                TempData["ErrorMessage"] =
                    "الدواء غير موجود أو غير متاح.";

                return RedirectToAction(
                    nameof(Index));
            }

            // -----------------------------------------------------
            // Get batches
            // -----------------------------------------------------
            var allBatches =
                await _unitOfWork
                    .MedicineBatches
                    .GetAllAsync();

            var today =
                DateTime.UtcNow.Date;

            // -----------------------------------------------------
            // Valid batches
            //
            // Expired batches are ignored.
            // Empty batches are ignored.
            // -----------------------------------------------------
            var validBatches =
                allBatches
                    .Where(b =>
                        b.MedicineId ==
                            medicineId &&

                        b.ExpiryDate >=
                            today &&

                        b.RemainingUnits > 0)
                    .OrderBy(b =>
                        b.ExpiryDate)
                    .ToList();

            // -----------------------------------------------------
            // Calculate available stock
            // -----------------------------------------------------
            var availableStock =
                validBatches.Sum(
                    b => b.RemainingUnits);

            if (availableStock <= 0)
            {
                TempData["ErrorMessage"] =
                    $"الدواء {medicine.Name} غير متوفر حالياً.";

                return RedirectToAction(
                    nameof(Index));
            }

            // -----------------------------------------------------
            // Get current cart
            // -----------------------------------------------------
            var cart =
                GetCart();

            var existingItem =
                cart.FirstOrDefault(
                    c =>
                        c.MedicineId ==
                        medicineId);

            // =====================================================
            // Medicine already exists in cart
            // =====================================================
            if (existingItem != null)
            {
                var newQuantity =
                    existingItem.Quantity +
                    quantity;

                if (newQuantity >
                    availableStock)
                {
                    TempData["ErrorMessage"] =
                        $"الكمية المطلوبة أكبر من المخزون المتاح. " +
                        $"المتاح: {availableStock} وحدة.";

                    return RedirectToAction(
                        nameof(Cart));
                }

                existingItem.Quantity =
                    newQuantity;

                // -------------------------------------------------
                // Refresh display price
                //
                // The real price will still be read from DB during
                // checkout.
                // -------------------------------------------------
                var currentBatch =
                    validBatches
                        .FirstOrDefault();

                if (currentBatch != null)
                {
                    existingItem.UnitPrice =
                        currentBatch.SellingPrice;
                }
            }

            // =====================================================
            // New medicine
            // =====================================================
            else
            {
                // -------------------------------------------------
                // FEFO batch for display
                // -------------------------------------------------
                var activeBatch =
                    validBatches
                        .FirstOrDefault();

                var displayPrice =
                    activeBatch?.SellingPrice
                    ?? 0m;

                cart.Add(
                    new CartItemViewModel
                    {
                        MedicineId =
                            medicine.Id,

                        MedicineName =
                            medicine.Name,

                        ScientificName =
                            medicine.ScientificName
                            ?? string.Empty,

                        UnitPrice =
                            displayPrice,

                        Quantity =
                            quantity,

                        RequiresPrescription =
                            medicine.RequiresPrescription
                    });
            }

            // -----------------------------------------------------
            // Save cart
            // -----------------------------------------------------
            SaveCart(cart);

            TempData["SuccessMessage"] =
                $"تم إضافة {medicine.Name} إلى السلة بنجاح!";

            return RedirectToAction(
                nameof(Cart));
        }


        // =========================================================
        // POST: /Shop/RemoveFromCart
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(
            int medicineId)
        {
            var cart =
                GetCart();

            var item =
                cart.FirstOrDefault(
                    c =>
                        c.MedicineId ==
                        medicineId);

            if (item != null)
            {
                cart.Remove(item);

                SaveCart(cart);

                TempData["SuccessMessage"] =
                    "تم حذف الدواء من السلة.";
            }

            return RedirectToAction(
                nameof(Cart));
        }


        // =========================================================
        // POST: /Shop/Checkout
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            CartViewModel model,
            IFormFile? prescriptionFile)
        {
            // -----------------------------------------------------
            // Get cart from session
            // -----------------------------------------------------
            var cartItems =
                GetCart();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] =
                    "سلة المشتريات فارغة!";

                return RedirectToAction(
                    nameof(Cart));
            }

            // -----------------------------------------------------
            // Validate delivery address
            // -----------------------------------------------------
            var deliveryAddress =
                model.CheckoutInfo?
                    .DeliveryAddress?
                    .Trim();

            if (string.IsNullOrWhiteSpace(
                deliveryAddress))
            {
                TempData["ErrorMessage"] =
                    "يرجى إدخال عنوان التوصيل.";

                return RedirectToAction(
                    nameof(Cart));
            }

            // -----------------------------------------------------
            // Get active medicines
            // -----------------------------------------------------
            var medicines =
                await _unitOfWork
                    .Medicines
                    .GetAllAsync();

            var medicineDictionary =
                medicines
                    .Where(m => m.IsActive)
                    .ToDictionary(
                        m => m.Id);

            // -----------------------------------------------------
            // Validate cart medicines
            // -----------------------------------------------------
            var requiresPrescription =
                false;

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Quantity <= 0)
                {
                    TempData["ErrorMessage"] =
                        "الكمية يجب أن تكون أكبر من صفر.";

                    return RedirectToAction(
                        nameof(Cart));
                }

                if (!medicineDictionary.TryGetValue(
                        cartItem.MedicineId,
                        out var medicine))
                {
                    TempData["ErrorMessage"] =
                        "أحد الأدوية الموجودة في السلة غير متاح.";

                    return RedirectToAction(
                        nameof(Cart));
                }

                if (medicine.RequiresPrescription)
                {
                    requiresPrescription =
                        true;
                }
            }

            // -----------------------------------------------------
            // Prescription validation
            // -----------------------------------------------------
            if (requiresPrescription &&
                (prescriptionFile == null ||
                 prescriptionFile.Length == 0))
            {
                ModelState.AddModelError(
                    "",
                    "تحتوي السلة على أدوية تتطلب وصفة طبية. " +
                    "يرجى إرفاق الوصفة.");

                model.CartItems =
                    cartItems;

                model.DeliveryFee =
                    DeliveryFeeAmount;

                return View(
                    "Cart",
                    model);
            }

            string? prescriptionPath =
                null;

            // =====================================================
            // Upload prescription
            // =====================================================
            if (prescriptionFile != null &&
                prescriptionFile.Length > 0)
            {
                const long maxFileSize =
                    5 * 1024 * 1024;

                if (prescriptionFile.Length >
                    maxFileSize)
                {
                    TempData["ErrorMessage"] =
                        "حجم الوصفة الطبية يجب ألا يتجاوز 5 ميجابايت.";

                    return RedirectToAction(
                        nameof(Cart));
                }

                var allowedExtensions =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase)
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".pdf"
                    };

                var extension =
                    Path.GetExtension(
                        prescriptionFile.FileName);

                if (!allowedExtensions.Contains(
                    extension))
                {
                    TempData["ErrorMessage"] =
                        "نوع ملف الوصفة غير مسموح به.";

                    return RedirectToAction(
                        nameof(Cart));
                }

                var uploadsFolder =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "prescriptions");

                Directory.CreateDirectory(
                    uploadsFolder);

                var uniqueFileName =
                    $"{Guid.NewGuid():N}" +
                    extension.ToLowerInvariant();

                var filePath =
                    Path.Combine(
                        uploadsFolder,
                        uniqueFileName);

                await using var fileStream =
                    new FileStream(
                        filePath,
                        FileMode.CreateNew);

                await prescriptionFile
                    .CopyToAsync(fileStream);

                prescriptionPath =
                    $"/uploads/prescriptions/" +
                    uniqueFileName;
            }

            // =====================================================
            // DATABASE TRANSACTION
            // =====================================================
            await using var transaction =
                await _unitOfWork
                    .BeginTransactionAsync();

            try
            {
                decimal subTotal = 0m;

                var orderItems =
                    new List<OrderItem>();

                var stockTransactions =
                    new List<StockTransaction>();

                // -------------------------------------------------
                // IMPORTANT:
                // Load all batches ONCE.
                // -------------------------------------------------
                var allBatches =
                    await _unitOfWork
                        .MedicineBatches
                        .GetAllAsync();

                var today =
                    DateTime.UtcNow.Date;

                // =================================================
                // Process every cart item
                // =================================================
                foreach (var cartItem in cartItems)
                {
                    var medicine =
                        medicineDictionary[
                            cartItem.MedicineId];

                    // -------------------------------------------------
                    // Get valid batches
                    //
                    // FEFO:
                    // First Expired / First Expiring, First Out
                    // -------------------------------------------------
                    var batches =
                        allBatches
                            .Where(b =>
                                b.MedicineId ==
                                    cartItem.MedicineId &&

                                b.ExpiryDate >=
                                    today &&

                                b.RemainingUnits > 0)
                            .OrderBy(b =>
                                b.ExpiryDate)
                            .ToList();

                    var availableStock =
                        batches.Sum(
                            b => b.RemainingUnits);

                    // -------------------------------------------------
                    // Check stock
                    // -------------------------------------------------
                    if (availableStock <
                        cartItem.Quantity)
                    {
                        await _unitOfWork
                            .RollbackTransactionAsync();

                        TempData["ErrorMessage"] =
                            $"المخزون غير كافٍ للدواء " +
                            $"({medicine.Name}). " +
                            $"المتاح حالياً: " +
                            $"{availableStock} وحدة.";

                        return RedirectToAction(
                            nameof(Cart));
                    }

                    var remainingQuantity =
                        cartItem.Quantity;

                    // =================================================
                    // Deduct FEFO
                    // =================================================
                    foreach (var batch in batches)
                    {
                        if (remainingQuantity <= 0)
                            break;

                        var soldUnits =
                            Math.Min(
                                batch.RemainingUnits,
                                remainingQuantity);

                        // -------------------------------------------------
                        // Get prices from DB
                        //
                        // NEVER trust the price stored in Session.
                        // -------------------------------------------------
                        var unitSellingPrice =
                            batch.SellingPrice;

                        var unitCostPrice =
                            batch.PurchasePrice;

                        var itemTotal =
                            unitSellingPrice *
                            soldUnits;

                        // -------------------------------------------------
                        // Deduct stock
                        // -------------------------------------------------
                        batch.RemainingUnits -=
                            soldUnits;

                        _unitOfWork
                            .MedicineBatches
                            .Update(batch);

                        // -------------------------------------------------
                        // Create OrderItem
                        //
                        // One OrderItem for each batch.
                        // -------------------------------------------------
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

                        // -------------------------------------------------
                        // Add subtotal
                        // -------------------------------------------------
                        subTotal +=
                            itemTotal;

                        // -------------------------------------------------
                        // Stock Transaction
                        //
                        // Sale = negative quantity
                        // -------------------------------------------------
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
                                    "ONLINE",

                                Notes =
                                    $"Online sale - " +
                                    $"Medicine: {medicine.Name}",

                                CreatedAt =
                                    DateTime.UtcNow
                            });

                        remainingQuantity -=
                            soldUnits;
                    }
                }

                // =====================================================
                // Generate Order Number
                // =====================================================
                var orderNumber =
                    GenerateOrderNumber();

                // =====================================================
                // Create Order
                // =====================================================
                var order =
                    new Order
                    {
                        OrderNumber =
                            orderNumber,

                        OrderType =
                            OrderType.Online_Delivery,

                        OrderStatus =
                            OrderStatus.Pending,

                        PaymentStatus =
                            PaymentStatus.Unpaid,

                        PaymentMethod =
                            PaymentMethod.Cash,

                        SubTotal =
                            subTotal,

                        DiscountAmount =
                            0m,

                        DeliveryFee =
                            DeliveryFeeAmount,

                        TotalAmount =
                            subTotal +
                            DeliveryFeeAmount,

                        OrderDate =
                            DateTime.UtcNow,

                        ShippingAddress =
                            deliveryAddress,

                        OrderItems =
                            orderItems
                    };

                // =====================================================
                // Current Logged-in User
                // =====================================================
                var userId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(
                    userId))
                {
                    order.ClientId =
                        userId;
                }

                // =====================================================
                // Prescription
                // =====================================================
                if (!string.IsNullOrWhiteSpace(
                    prescriptionPath))
                {
                    order.Prescription =
                        new Prescription
                        {
                            ImagePath =
                                prescriptionPath,

                            UploadedAt =
                                DateTime.UtcNow,

                            Status =
                                PrescriptionStatus
                                    .PendingReview
                        };
                }

                // =====================================================
                // Save Order
                // =====================================================
                await _unitOfWork
                    .Orders
                    .AddAsync(order);

                // =====================================================
                // Save Stock Transactions
                // =====================================================
                foreach (
                    var stockTransaction
                    in stockTransactions)
                {
                    await _unitOfWork
                        .StockTransactions
                        .AddAsync(
                            stockTransaction);
                }

                // =====================================================
                // Save all changes
                // =====================================================
                await _unitOfWork
                    .CompleteAsync();

                // =====================================================
                // Commit
                //
                // CommitTransactionAsync() must NOT call
                // SaveChangesAsync().
                // =====================================================
                await _unitOfWork
                    .CommitTransactionAsync();

                // =====================================================
                // Clear cart after successful commit
                // =====================================================
                HttpContext.Session
                    .Remove(
                        CartSessionKey);

                TempData["SuccessMessage"] =
                    $"تم إرسال طلبك بنجاح! " +
                    $"رقم الطلب: #{order.OrderNumber}";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception)
            {
                // -------------------------------------------------
                // Rollback database transaction
                // -------------------------------------------------
                await _unitOfWork
                    .RollbackTransactionAsync();

                // -------------------------------------------------
                // Delete uploaded prescription if DB failed
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    prescriptionPath))
                {
                    var uploadedFilePath =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            prescriptionPath
                                .TrimStart('/')
                                .Replace(
                                    "/",
                                    Path.DirectorySeparatorChar
                                        .ToString()));

                    if (System.IO.File.Exists(
                        uploadedFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(
                                uploadedFilePath);
                        }
                        catch
                        {
                            // Do not hide original error.
                        }
                    }
                }

                TempData["ErrorMessage"] =
                    "حدث خطأ أثناء حفظ الطلب. " +
                    "يرجى المحاولة مرة أخرى.";

                return RedirectToAction(
                    nameof(Cart));
            }
        }


        // =========================================================
        // GET: /Shop/Details/5
        // =========================================================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var medicine =
                await _unitOfWork
                    .Medicines
                    .GetByIdAsync(id);

            if (medicine == null ||
                !medicine.IsActive)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Load batches so Details can also show:
            // - Price
            // - Stock
            // - Expiry
            // -----------------------------------------------------
            var allBatches =
                await _unitOfWork
                    .MedicineBatches
                    .GetAllAsync();

            medicine.Batches =
                allBatches
                    .Where(b =>
                        b.MedicineId == id)
                    .OrderBy(b =>
                        b.ExpiryDate)
                    .ToList();

            return View(medicine);
        }


        // =========================================================
        // Helper: Get Cart
        // =========================================================
        private List<CartItemViewModel> GetCart()
        {
            return
                HttpContext.Session
                    .GetObjectFromJson<
                        List<CartItemViewModel>>(
                            CartSessionKey)
                ?? new List<CartItemViewModel>();
        }


        // =========================================================
        // Helper: Save Cart
        // =========================================================
        private void SaveCart(
            List<CartItemViewModel> cart)
        {
            HttpContext.Session
                .SetObjectAsJson(
                    CartSessionKey,
                    cart);
        }


        // =========================================================
        // Helper: Generate Order Number
        //
        // Maximum = 20 characters
        //
        // Example:
        // ON-260914-153012-A1B2C3
        //
        // Length = 20
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