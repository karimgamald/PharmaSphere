using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Web.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class MedicineController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicineController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Medicine
        public async Task<IActionResult> Index()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync(
                query => query
                    .Include(m => m.Batches)
                    .Include(m => m.Category)
            );

            return View(medicines);
        }

        // GET: /Medicine/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();

            var viewModel = new MedicineFormViewModel
            {
                Categories = categories
                    .Where(c => c.IsActive)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
            };

            return View(viewModel);
        }

        // POST: /Medicine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicineFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories(model);
                return View(model);
            }

            var category = await _unitOfWork.Categories
                .GetByIdAsync(model.CategoryId);

            if (category == null || !category.IsActive)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "القسم غير موجود أو غير نشط.");

                await LoadCategories(model);
                return View(model);
            }

            var medicine = new Medicine
            {
                Name = model.Name,
                ScientificName = model.ScientificName,
                ActiveIngredient = model.ActiveIngredient,
                ActiveIngredientConcentration =
                    model.ActiveIngredientConcentration,
                Barcode = model.Barcode,
                UnitsPerBox = model.UnitsPerBox > 0
                    ? model.UnitsPerBox
                    : 1,
                RequiresPrescription = model.RequiresPrescription,
                ImagePath = model.ImagePath,
                CategoryId = model.CategoryId,
                IsActive = true
            };

            await _unitOfWork.Medicines.AddAsync(medicine);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] =
                "تم إضافة الدواء بنجاح. يمكنك الآن إضافة المخزون من خلال أمر شراء.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Medicine/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id);

            if (medicine == null)
                return NotFound();

            var categories = await _unitOfWork.Categories.GetAllAsync();

            var viewModel = new MedicineFormViewModel
            {
                Id = medicine.Id,
                Name = medicine.Name,
                ScientificName = medicine.ScientificName,
                ActiveIngredient = medicine.ActiveIngredient,
                ActiveIngredientConcentration =
                    medicine.ActiveIngredientConcentration,
                Barcode = medicine.Barcode,
                UnitsPerBox = medicine.UnitsPerBox,
                RequiresPrescription = medicine.RequiresPrescription,
                ImagePath = medicine.ImagePath,
                CategoryId = medicine.CategoryId,

                Categories = categories
                    .Where(c => c.IsActive)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
            };

            return View(viewModel);
        }

        // POST: /Medicine/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            MedicineFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadCategories(model);
                return View(model);
            }

            var medicine =
                await _unitOfWork.Medicines.GetByIdAsync(id);

            if (medicine == null)
                return NotFound();

            var category =
                await _unitOfWork.Categories.GetByIdAsync(model.CategoryId);

            if (category == null || !category.IsActive)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "القسم غير موجود أو غير نشط.");

                await LoadCategories(model);
                return View(model);
            }

            medicine.Name = model.Name;
            medicine.ScientificName = model.ScientificName;
            medicine.ActiveIngredient = model.ActiveIngredient;
            medicine.ActiveIngredientConcentration =
                model.ActiveIngredientConcentration;
            medicine.Barcode = model.Barcode;
            medicine.UnitsPerBox = model.UnitsPerBox > 0
                ? model.UnitsPerBox
                : 1;
            medicine.RequiresPrescription =
                model.RequiresPrescription;
            medicine.ImagePath = model.ImagePath;
            medicine.CategoryId = model.CategoryId;

            _unitOfWork.Medicines.Update(medicine);

            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] =
                "تم تحديث بيانات الدواء بنجاح!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Medicine/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(
                id,
                query => query
                    .Include(m => m.Batches)
                    .Include(m => m.Category)
            );

            if (medicine == null)
                return NotFound();

            return View(medicine);
        }

        // GET: /Medicine/ConfirmDeactivate/5
        [HttpGet]
        public async Task<IActionResult> ConfirmDeactivate(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(
                id,
                query => query
                    .Include(m => m.Batches)
                    .Include(m => m.Category)
            );

            if (medicine == null)
                return NotFound();

            if (!medicine.IsActive)
                return RedirectToAction(nameof(Index));

            return View("Deactivate", medicine);
        }

        // POST: /Medicine/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(
                id,
                query => query
                    .Include(m => m.Batches)
                    .Include(m => m.Category)
            );

            if (medicine == null)
                return NotFound();

            medicine.IsActive = false;

            _unitOfWork.Medicines.Update(medicine);

            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] =
                "تم تعطيل الدواء بنجاح!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Medicine/ConfirmActivate/5
        [HttpGet]
        public async Task<IActionResult> ConfirmActivate(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(
                id,
                query => query
                    .Include(m => m.Batches)
                    .Include(m => m.Category)
            );

            if (medicine == null)
                return NotFound();

            if (medicine.IsActive)
                return RedirectToAction(nameof(Index));

            return View("Activate", medicine);
        }

        // POST: /Medicine/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(
                id,
                query => query
                    .Include(m => m.Batches)
                    .Include(m => m.Category)
            );

            if (medicine == null)
                return NotFound();

            medicine.IsActive = true;

            _unitOfWork.Medicines.Update(medicine);

            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] =
                "تم تفعيل الدواء بنجاح!";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> UnitUnitsConfig()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();

            var model = medicines
                .Select(x => new UnitUnitsConfigViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ScientificName = x.ScientificName,
                    Barcode = x.Barcode,
                    UnitsPerBox = x.UnitsPerBox
                })
                .ToList();

            return View(model);
        }
        // Helper method
        private async Task LoadCategories(
            MedicineFormViewModel model)
        {
            var categories =
                await _unitOfWork.Categories.GetAllAsync();

            model.Categories = categories
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });
        }
    }
}