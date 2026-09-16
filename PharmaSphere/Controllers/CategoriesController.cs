using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using System.Threading.Tasks;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();

            return View(categories);
        }

        // GET: /Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.IsActive = true;

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "تم إضافة القسم بنجاح!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(category);

            var existingCategory =
                await _unitOfWork.Categories.GetByIdAsync(id);

            if (existingCategory == null)
                return NotFound();

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;

            _unitOfWork.Categories.Update(existingCategory);

            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "تم تحديث القسم بنجاح!";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Categories/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            category.IsActive = false;

            _unitOfWork.Categories.Update(category);

            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "تم تعطيل القسم بنجاح!";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Categories/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            category.IsActive = true;

            _unitOfWork.Categories.Update(category);

            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "تم تفعيل القسم بنجاح!";

            return RedirectToAction(nameof(Index));
        }
    }
}