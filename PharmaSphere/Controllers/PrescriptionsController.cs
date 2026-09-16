using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaSphere.Web.Controllers
{
    //[Authorize]
    public class PrescriptionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".pdf"
        };

        public PrescriptionsController(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
            _userManager = userManager;
        }


        // =========================================================
        // INDEX
        // GET: /Prescriptions
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var prescriptions =
                await _unitOfWork.Prescriptions.GetAllAsync(
                    query => query
                        .Include(p => p.Client)
                        .Include(p => p.Order)
                        .OrderByDescending(p => p.UploadedAt)
                );

            return View(prescriptions);
        }


        // =========================================================
        // DETAILS
        // GET: /Prescriptions/Details/5
        // =========================================================

        public async Task<IActionResult> Details(int id)
        {
            var prescription =
                await _unitOfWork.Prescriptions.GetByIdAsync(
                    id,
                    query => query
                        .Include(p => p.Client)
                        .Include(p => p.Order)
                        .Include(p => p.ReviewedBy)
                );

            if (prescription == null)
                return NotFound();

            return View(prescription);
        }


        // =========================================================
        // REVIEW - GET
        // GET: /Prescriptions/Review/5
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var prescription =
                await _unitOfWork.Prescriptions.GetByIdAsync(
                    id,
                    query => query
                        .Include(p => p.Client)
                        .Include(p => p.Order)
                        .Include(p => p.ReviewedBy)
                );

            if (prescription == null)
                return NotFound();

            return View(prescription);
        }


        // =========================================================
        // REVIEW - POST
        // POST: /Prescriptions/Review
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(
            int id,
            PrescriptionStatus status,
            string? pharmacistNotes)
        {
            var prescription =
                await _unitOfWork.Prescriptions.GetByIdAsync(id);

            if (prescription == null)
                return NotFound();


            // -----------------------------------------------------
            // Validate status
            // -----------------------------------------------------

            if (status != PrescriptionStatus.Processed &&
                status != PrescriptionStatus.Rejected)
            {
                TempData["ErrorMessage"] =
                    "حالة الروشتة غير صحيحة.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // -----------------------------------------------------
            // Get current Identity User ID
            // -----------------------------------------------------

            var pharmacistId =
                _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(pharmacistId))
            {
                return Unauthorized();
            }


            // -----------------------------------------------------
            // Make sure reviewer actually exists
            // -----------------------------------------------------

            var pharmacist =
                await _userManager.FindByIdAsync(pharmacistId);

            if (pharmacist == null)
            {
                return Unauthorized();
            }


            // -----------------------------------------------------
            // Update prescription
            // -----------------------------------------------------

            prescription.Status = status;

            prescription.PharmacistNotes =
                pharmacistNotes?.Trim();

            prescription.ReviewedById =
                pharmacist.Id;

            prescription.ReviewedAt =
                DateTime.UtcNow;


            _unitOfWork.Prescriptions.Update(prescription);

            await _unitOfWork.CompleteAsync();


            // -----------------------------------------------------
            // Success message
            // -----------------------------------------------------

            TempData["SuccessMessage"] =
                status == PrescriptionStatus.Processed
                    ? "تم اعتماد الروشتة بنجاح."
                    : "تم رفض الروشتة.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =========================================================
        // UPLOAD - GET
        // GET: /Prescriptions/Upload
        // =========================================================

        [HttpGet]
        public IActionResult Upload(int? orderId = null)
        {
            ViewBag.OrderId = orderId;

            return View();
        }


        // =========================================================
        // UPLOAD - POST
        // POST: /Prescriptions/Upload
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            IFormFile file,
            int? orderId = null)
        {
            // -----------------------------------------------------
            // Validate file
            // -----------------------------------------------------

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(
                    "file",
                    "من فضلك اختر صورة أو ملف الروشتة.");

                ViewBag.OrderId = orderId;

                return View();
            }


            // -----------------------------------------------------
            // Validate file size
            // -----------------------------------------------------

            if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError(
                    "file",
                    "حجم الملف يجب ألا يتجاوز 5 ميجابايت.");

                ViewBag.OrderId = orderId;

                return View();
            }


            // -----------------------------------------------------
            // Validate extension
            // -----------------------------------------------------

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "file",
                    "نوع الملف غير مسموح. الأنواع المسموحة: JPG, JPEG, PNG, WEBP, PDF.");

                ViewBag.OrderId = orderId;

                return View();
            }


            // -----------------------------------------------------
            // Get current Identity User ID
            // -----------------------------------------------------

            var clientId =
                _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return Unauthorized();
            }


            // -----------------------------------------------------
            // Make sure client exists
            // -----------------------------------------------------

            var client =
                await _userManager.FindByIdAsync(clientId);

            if (client == null)
            {
                return Unauthorized();
            }


            // -----------------------------------------------------
            // Validate Order if provided
            // -----------------------------------------------------

            Order? order = null;

            if (orderId.HasValue)
            {
                order =
                    await _unitOfWork.Orders.GetByIdAsync(
                        orderId.Value);

                if (order == null)
                {
                    ModelState.AddModelError(
                        "orderId",
                        "الطلب غير موجود.");

                    ViewBag.OrderId = orderId;

                    return View();
                }


                // The order must belong to the current client
                if (order.ClientId != client.Id)
                {
                    return Forbid();
                }
            }


            // -----------------------------------------------------
            // Create upload directory
            // -----------------------------------------------------

            var uploadFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "prescriptions");


            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }


            // -----------------------------------------------------
            // Generate safe unique file name
            // -----------------------------------------------------

            var fileName =
                $"{Guid.NewGuid():N}{extension}";


            var filePath =
                Path.Combine(
                    uploadFolder,
                    fileName);


            // -----------------------------------------------------
            // Save file
            // -----------------------------------------------------

            await using (
                var stream =
                    new FileStream(
                        filePath,
                        FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }


            // -----------------------------------------------------
            // Create Prescription
            // -----------------------------------------------------

            var prescription =
                new Prescription
                {
                    ImagePath =
                        $"/uploads/prescriptions/{fileName}",

                    Status =
                        PrescriptionStatus.PendingReview,

                    UploadedAt =
                        DateTime.UtcNow,

                    ClientId =
                        client.Id,

                    OrderId =
                        orderId
                };


            await _unitOfWork.Prescriptions
                .AddAsync(prescription);


            await _unitOfWork.CompleteAsync();


            // -----------------------------------------------------
            // Success message
            // -----------------------------------------------------

            TempData["SuccessMessage"] =
                "تم رفع الروشتة بنجاح، وهي الآن قيد المراجعة.";


            return RedirectToAction(
                nameof(Details),
                new { id = prescription.Id });
        }
    }
}