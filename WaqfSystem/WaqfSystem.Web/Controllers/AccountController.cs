using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Core.Entities;
using WaqfSystem.Web.ViewModels;

namespace WaqfSystem.Web.Controllers
{
    public class AccountController : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Stub for authentication logic
                if (model.Email == "admin@waqf.gov.iq" && model.Password == "Admin123")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "مدير النظام"),
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Role, "SYS_ADMIN"),
                        new Claim("GovernorateId", "1")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    SuccessMessage("مرحباً بك في نظام حصر الأملاك");
                    return LocalRedirect(model.ReturnUrl ?? "/");
                }
                
                ErrorMessage("خطأ في اسم المستخدم أو كلمة المرور");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
