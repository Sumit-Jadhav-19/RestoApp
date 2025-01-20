using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestoApp.Models;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestoApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (User.IsInRole("Captain"))
                {
                    return RedirectToAction("Tables", "Captain");
                }
                else if (User.IsInRole("Manager"))
                {
                    return RedirectToAction("", "");
                }
                else if (User.IsInRole("Chef"))
                {
                    return RedirectToAction("Index", "Kitchen");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            if (result.IsLockedOut)
            {
                return View("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
