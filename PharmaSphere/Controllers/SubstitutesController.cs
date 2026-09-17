using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Web.ViewModels;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class SubstitutesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubstitutesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // GET: /Substitutes
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(int? medicineId)
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();

            ViewBag.Medicines = medicines
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToList();

            var model = new SubstituteSearchViewModel();

            if (!medicineId.HasValue)
                return View(model);

            var medicine = await _unitOfWork.Medicines.GetByIdAsync(
                medicineId.Value,
                query => query
                    .Include(x => x.Batches)
                    .Include(x => x.Category)
            );

            if (medicine == null)
            {
                TempData["Error"] = "الدواء غير موجود.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(medicine.ActiveIngredient))
            {
                model.MedicineId = medicine.Id;
                model.MedicineName = medicine.Name;
                model.ActiveIngredient = medicine.ActiveIngredient;
                model.Concentration =
                    medicine.ActiveIngredientConcentration;

                TempData["Warning"] =
                    "هذا الدواء لا يحتوي على مادة فعالة مسجلة، لذلك لا يمكن البحث عن بدائل.";

                return View(model);
            }

            // =====================================================
            // Find substitutes
            // =====================================================

            var allMedicines = await _unitOfWork.Medicines.GetAllAsync(
                query => query
                    .Include(x => x.Batches)
            );

            var substitutes = allMedicines
                .Where(x =>
                    x.IsActive &&
                    x.Id != medicine.Id &&
                    !string.IsNullOrWhiteSpace(x.ActiveIngredient) &&
                    x.ActiveIngredient.Trim()
                        .Equals(
                            medicine.ActiveIngredient.Trim(),
                            StringComparison.OrdinalIgnoreCase
                        ) &&
                    (
                        string.IsNullOrWhiteSpace(
                            medicine.ActiveIngredientConcentration
                        )
                        ||
                        string.Equals(
                            x.ActiveIngredientConcentration?.Trim(),
                            medicine.ActiveIngredientConcentration?.Trim(),
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                .Select(x =>
                {
                    var availableBatches = x.Batches?
                        .Where(b =>
                            b.RemainingUnits > 0 &&
                            b.ExpiryDate.Date >= DateTime.UtcNow.Date)
                        .OrderBy(b => b.ExpiryDate)
                        .ToList()
                        ?? new List<MedicineBatch>();

                    return new SubstituteMedicineViewModel
                    {
                        MedicineId = x.Id,

                        Name = x.Name,

                        ScientificName = x.ScientificName,

                        ActiveIngredient = x.ActiveIngredient,

                        Concentration =
                            x.ActiveIngredientConcentration,

                        Barcode = x.Barcode,

                        RequiresPrescription =
                            x.RequiresPrescription,

                        TotalStock = availableBatches.Sum(
                            b => b.RemainingUnits
                        ),

                        LowestSellingPrice =
                            availableBatches
                                .Select(b => (decimal?)b.SellingPrice)
                                .Min(),

                        NearestExpiryDate =
                            availableBatches
                                .Select(b => (DateTime?)b.ExpiryDate)
                                .Min()
                    };
                })
                .OrderByDescending(x => x.TotalStock > 0)
                .ThenBy(x => x.Name)
                .ToList();

            model.MedicineId = medicine.Id;

            model.MedicineName = medicine.Name;

            model.ActiveIngredient =
                medicine.ActiveIngredient;

            model.Concentration =
                medicine.ActiveIngredientConcentration;

            model.Substitutes = substitutes;

            return View(model);
        }
    }
}