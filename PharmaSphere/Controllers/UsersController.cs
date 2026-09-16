using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using PharmaSphere.Web.ViewModels;

namespace PharmaSphere.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "Client",
                    CreatedAt = user.CreatedAt,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return View(model);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserViewModel
            {
                Role = UserRole.Client
            });
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(
                model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "هذا البريد الإلكتروني مستخدم بالفعل.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,

                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,

                Role = model.Role,

                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            var roleName = model.Role.ToString();

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                roleName);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            TempData["Success"] =
                $"تم إنشاء حساب {GetArabicRoleName(model.Role)} بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var role = ParseRole(
                roles.FirstOrDefault());

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = role
            };

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            // Prevent Admin from removing his own Admin role
            if (user.Id == _userManager.GetUserId(User) &&
                model.Role != UserRole.Admin)
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "لا يمكنك إزالة دور Admin من حسابك الحالي.");

                return View(model);
            }

            var emailOwner = await _userManager.FindByEmailAsync(
                model.Email);

            if (emailOwner != null &&
                emailOwner.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "هذا البريد الإلكتروني مستخدم بالفعل.");

                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            if (user.Email != model.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NormalizedEmail =
                    _userManager.NormalizeEmail(model.Email);
                user.NormalizedUserName =
                    _userManager.NormalizeName(model.Email);
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            var newRole = model.Role.ToString();

            if (!currentRoles.Contains(newRole))
            {
                var removeResult =
                    await _userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);

                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }

                var addResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        newRole);

                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }

                user.Role = model.Role;
                await _userManager.UpdateAsync(user);
            }

            TempData["Success"] =
                "تم تحديث بيانات المستخدم بنجاح.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            // Prevent deleting yourself
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] =
                    "لا يمكنك حذف حسابك الحالي.";

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] =
                    "لا يمكنك حذف حسابك الحالي.";

                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "تم حذف المستخدم بنجاح.";
            }
            else
            {
                TempData["Error"] =
                    "حدث خطأ أثناء حذف المستخدم.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static UserRole ParseRole(string? role)
        {
            if (Enum.TryParse<UserRole>(
                role,
                true,
                out var result))
            {
                return result;
            }

            return UserRole.Client;
        }

        private static string GetArabicRoleName(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "مدير النظام",
                UserRole.Pharmacist => "صيدلي",
                UserRole.Delivery => "مندوب توصيل",
                UserRole.Client => "عميل",
                _ => "مستخدم"
            };
        }
    }
}