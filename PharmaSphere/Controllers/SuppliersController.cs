using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class SuppliersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(
            IUnitOfWork unitOfWork,
            ILogger<SuppliersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetAllAsync();

                var orderedSuppliers = suppliers
                    .OrderByDescending(s => s.IsActive)
                    .ThenBy(s => s.Name)
                    .ToList();

                return View(orderedSuppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading suppliers.");

                TempData["ErrorMessage"] =
                    "حدث خطأ أثناء تحميل بيانات الموردين.";

                return View(new List<Supplier>());
            }
        }

        // GET: Suppliers/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            // Normalize input
            supplier.Name = supplier.Name?.Trim() ?? string.Empty;
            supplier.Phone = supplier.Phone?.Trim();
            supplier.Email = supplier.Email?.Trim();
            supplier.Address = supplier.Address?.Trim();

            if (string.IsNullOrWhiteSpace(supplier.Name))
            {
                ModelState.AddModelError(
                    nameof(supplier.Name),
                    "اسم المورد مطلوب.");
            }

            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            try
            {
                // Prevent duplicate supplier names
                var existingSuppliers =
                    await _unitOfWork.Suppliers.GetAllAsync();

                var duplicateExists = existingSuppliers.Any(s =>
                    s.Name.Equals(
                        supplier.Name,
                        StringComparison.OrdinalIgnoreCase));

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(supplier.Name),
                        "يوجد مورد بهذا الاسم بالفعل.");

                    return View(supplier);
                }

                // New suppliers are active by default
                supplier.IsActive = true;

                await _unitOfWork.Suppliers.AddAsync(supplier);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] =
                    "تم إضافة المورد بنجاح.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating supplier {SupplierName}.",
                    supplier.Name);

                ModelState.AddModelError(
                    string.Empty,
                    "حدث خطأ أثناء إضافة المورد.");

                return View(supplier);
            }
        }

        // GET: Suppliers/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var supplier =
                await _unitOfWork.Suppliers.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Supplier supplier)
        {
            if (id != supplier.Id)
                return BadRequest();

            // Normalize input
            supplier.Name = supplier.Name?.Trim() ?? string.Empty;
            supplier.Phone = supplier.Phone?.Trim();
            supplier.Email = supplier.Email?.Trim();
            supplier.Address = supplier.Address?.Trim();

            if (string.IsNullOrWhiteSpace(supplier.Name))
            {
                ModelState.AddModelError(
                    nameof(supplier.Name),
                    "اسم المورد مطلوب.");
            }

            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            try
            {
                var existingSupplier =
                    await _unitOfWork.Suppliers.GetByIdAsync(id);

                if (existingSupplier == null)
                    return NotFound();

                // Prevent duplicate names
                var suppliers =
                    await _unitOfWork.Suppliers.GetAllAsync();

                var duplicateExists = suppliers.Any(s =>
                    s.Id != id &&
                    s.Name.Equals(
                        supplier.Name,
                        StringComparison.OrdinalIgnoreCase));

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(supplier.Name),
                        "يوجد مورد آخر بهذا الاسم بالفعل.");

                    return View(supplier);
                }

                // Update editable fields only
                existingSupplier.Name = supplier.Name;
                existingSupplier.Phone = supplier.Phone;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Address = supplier.Address;

                // Don't modify IsActive here.
                // Activation/deactivation has separate actions.

                _unitOfWork.Suppliers.Update(existingSupplier);

                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] =
                    "تم تحديث بيانات المورد بنجاح.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating supplier {SupplierId}.",
                    id);

                ModelState.AddModelError(
                    string.Empty,
                    "حدث خطأ أثناء تحديث بيانات المورد.");

                return View(supplier);
            }
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmDeactivate(int id)
        {
            if (id <= 0)
                return BadRequest();

            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            if (!supplier.IsActive)
            {
                TempData["ErrorMessage"] = "المورد غير نشط بالفعل.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }
        // POST: Suppliers/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var supplier =
                    await _unitOfWork.Suppliers.GetByIdAsync(id);

                if (supplier == null)
                    return NotFound();

                if (!supplier.IsActive)
                {
                    TempData["ErrorMessage"] =
                        "المورد غير نشط بالفعل.";

                    return RedirectToAction(nameof(Index));
                }

                supplier.IsActive = false;

                _unitOfWork.Suppliers.Update(supplier);

                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] =
                    "تم تعطيل المورد بنجاح.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deactivating supplier {SupplierId}.",
                    id);

                TempData["ErrorMessage"] =
                    "حدث خطأ أثناء تعطيل المورد.";

                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmActivate(int id)
        {
            if (id <= 0)
                return BadRequest();

            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            if (supplier.IsActive)
            {
                TempData["ErrorMessage"] = "المورد نشط بالفعل.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }
        // POST: Suppliers/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var supplier =
                    await _unitOfWork.Suppliers.GetByIdAsync(id);

                if (supplier == null)
                    return NotFound();

                if (supplier.IsActive)
                {
                    TempData["ErrorMessage"] =
                        "المورد نشط بالفعل.";

                    return RedirectToAction(nameof(Index));
                }

                supplier.IsActive = true;

                _unitOfWork.Suppliers.Update(supplier);

                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] =
                    "تم تفعيل المورد بنجاح.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while activating supplier {SupplierId}.",
                    id);

                TempData["ErrorMessage"] =
                    "حدث خطأ أثناء تفعيل المورد.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}