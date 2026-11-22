using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers
{
    // Giriş, kayıt ve çıkış işlemlerini yöneten controller
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;     // kullanıcı oluşturma, rol atama
        private readonly SignInManager<ApplicationUser> _signInManager; // giriş / çıkış (çerez) işlemleri

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Sunucu tarafı doğrulama: form kuralları sağlanmadıysa formu tekrar göster
            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcı adı olarak e-postayı kullanıyoruz
            var sonuc = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (sonuc.Succeeded)
            {
                // Özel bir sayfadan gelinmediyse kullanıcıyı rolüne göre kendi paneline gönder
                if (string.IsNullOrEmpty(returnUrl))
                {
                    var kullanici = await _userManager.FindByEmailAsync(model.Email);
                    if (kullanici != null)
                    {
                        if (await _userManager.IsInRoleAsync(kullanici, Roles.Admin))
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        if (await _userManager.IsInRoleAsync(kullanici, Roles.Veteriner))
                            return RedirectToAction("Index", "Dashboard", new { area = "Vet" });
                        if (await _userManager.IsInRoleAsync(kullanici, Roles.Uye))
                            return RedirectToAction("Index", "Panel");
                    }
                }
                return YerelAdreseDon(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var kullanici = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var sonuc = await _userManager.CreateAsync(kullanici, model.Password);

            if (sonuc.Succeeded)
            {
                // Siteden kayıt olan herkes "Üye" (hayvan sahibi) rolünü alır
                await _userManager.AddToRoleAsync(kullanici, Roles.Uye);

                await _signInManager.SignInAsync(kullanici, isPersistent: false);
                TempData["Basari"] = "Kaydınız başarıyla oluşturuldu. Hoş geldiniz!";
                return RedirectToAction("Index", "Panel");
            }

            // Identity hatalarını (ör. e-posta zaten kayıtlı) forma ekle
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError(string.Empty, hata.Description);

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied  (yetkisi olmayan sayfaya girilince)
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Güvenlik: yalnızca kendi sitemizdeki bir adrese yönlendir (open redirect saldırısını önler)
        private IActionResult YerelAdreseDon(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
