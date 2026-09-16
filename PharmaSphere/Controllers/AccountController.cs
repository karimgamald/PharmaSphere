using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using PharmaSphere.Web.ViewModels;

namespace PharmaSphere.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        // =====================================================
        // GET: /Account/Login
        // =====================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }


        // =====================================================
        // POST: /Account/Login
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);


            var user = await _userManager.FindByEmailAsync(
                model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "البريد الإلكتروني أو كلمة المرور غير صحيحة.");

                return View(model);
            }


            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);


            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }


            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "تم قفل الحساب مؤقتًا بسبب محاولات تسجيل دخول فاشلة متكررة.");

                return View(model);
            }


            ModelState.AddModelError(
                string.Empty,
                "البريد الإلكتروني أو كلمة المرور غير صحيحة.");

            return View(model);
        }


        // =====================================================
        // GET: /Account/Register
        // =====================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // =====================================================
        // POST: /Account/Register
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            // =================================================
            // Create User
            // =================================================

            var user = new ApplicationUser
            {
                UserName = model.Email,

                Email = model.Email,

                FullName = model.FullName,

                PhoneNumber = model.PhoneNumber,

                Address = model.Address,

                Role = UserRole.Client,

                CreatedAt = DateTime.UtcNow
            };


            var result = await _userManager.CreateAsync(
                user,
                model.Password);


            if (result.Succeeded)
            {
                // المستخدم العادي يبدأ كـ Client
                await _userManager.AddToRoleAsync(
                    user,
                    "Client");


                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);


                return RedirectToAction(
                    "Index",
                    "Home");
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }


            return View(model);
        }


        // =====================================================
        // POST: /Account/Logout
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =====================================================
        // GET: /Account/AccessDenied
        // =====================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}