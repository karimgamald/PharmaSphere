using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaSphere.Web.Controllers
{
    public class MedicineBatchesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicineBatchesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /MedicineBatches
        public async Task<IActionResult> Index()
        {
            var batches = await _unitOfWork.MedicineBatches.GetAllAsync(
                query => query.Include(b => b.Medicine)
            );
            return View(batches);
        }

        // GET: /MedicineBatch/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new BatchFormViewModel
            {
                ExpiryDate = DateTime.Today.AddYears(1)
            };

            await LoadMedicinesAsync(viewModel);
            return View(viewModel);
        }

        // POST: /MedicineBatch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BatchFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadMedicinesAsync(model);
                return View(model);
            }

            var medicine = await _unitOfWork.Medicines.GetByIdAsync(model.MedicineId);
            if (medicine == null || !medicine.IsActive)
            {
                ModelState.AddModelError(nameof(model.MedicineId), "الدواء المحدد غير موجود أو غير نشط.");
                await LoadMedicinesAsync(model);
                return View(model);
            }

            var batch = new MedicineBatch
            {
                MedicineId = model.MedicineId,
                BatchNumber = model.BatchNumber,
                ExpiryDate = model.ExpiryDate,
                OriginalUnits = model.Quantity,
                RemainingUnits = model.Quantity,
                PurchasePrice = model.PurchasePrice,
                SellingPrice = model.SellingPrice,
                PurchaseOrderId = model.PurchaseOrderId
            };

            await _unitOfWork.MedicineBatches.AddAsync(batch);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "تم إضافة التشغيلة بنجاح!";
            return RedirectToAction(nameof(Index));
        }
        // GET: /MedicineBatches/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            var viewModel = new BatchFormViewModel
            {
                Id = batch.Id,
                MedicineId = batch.MedicineId,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate,
                Quantity = batch.RemainingUnits,
                PurchasePrice = batch.PurchasePrice,
                SellingPrice = batch.SellingPrice,
                PurchaseOrderId = batch.PurchaseOrderId
            };

            await LoadMedicinesAsync(viewModel);
            return View(viewModel);
        }

        // POST: /MedicineBatches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BatchFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadMedicinesAsync(model);
                return View(model);
            }

            var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            var medicine = await _unitOfWork.Medicines.GetByIdAsync(model.MedicineId);
            if (medicine == null || !medicine.IsActive)
            {
                ModelState.AddModelError(nameof(model.MedicineId), "الدواء المحدد غير موجود أو غير نشط.");
                await LoadMedicinesAsync(model);
                return View(model);
            }

            // تحديث بيانات التشغيلة
            batch.MedicineId = model.MedicineId;
            batch.BatchNumber = model.BatchNumber;
            batch.ExpiryDate = model.ExpiryDate;
            batch.RemainingUnits = model.Quantity;
            batch.PurchasePrice = model.PurchasePrice;
            batch.SellingPrice = model.SellingPrice;
            batch.PurchaseOrderId = model.PurchaseOrderId;

            _unitOfWork.MedicineBatches.Update(batch);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = "تم تعديل بيانات التشغيلة بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // Helper Method
        private async Task LoadMedicinesAsync(BatchFormViewModel model)
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();

            // التعيين للخاصية Medicines
            model.Medicines = medicines
                .Where(m => m.IsActive)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name,
                    Selected = m.Id == model.MedicineId
                });
        }
    }
}