using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager1.Models;
using PersonalFinanceManager1.Services;
using PersonalFinanceManager1.ViewModels;

namespace PersonalFinanceManager1.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDemoDataSeeder _demoDataSeeder;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IDemoDataSeeder demoDataSeeder)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _demoDataSeeder = demoDataSeeder;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User)) return RedirectToLocal(returnUrl);
        return View(new AuthPageViewModel { ActiveTab = "login" });
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User)) return RedirectToLocal(returnUrl);
        return View("Login", new AuthPageViewModel { ActiveTab = "register" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([Bind(Prefix = "Login")] LoginViewModel form, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(new AuthPageViewModel { ActiveTab = "login", Login = form });

        var result = await _signInManager.PasswordSignInAsync(form.Email, form.Password, form.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return RedirectToLocal(returnUrl);

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "تم قفل الحساب مؤقتاً بسبب محاولات دخول فاشلة متكررة. حاول لاحقاً."
            : "البريد الإلكتروني أو كلمة المرور غير صحيحة");

        return View(new AuthPageViewModel { ActiveTab = "login", Login = form });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind(Prefix = "Register")] RegisterViewModel form, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View("Login", new AuthPageViewModel { ActiveTab = "register", Register = form });

        var user = new ApplicationUser
        {
            UserName = form.Email,
            Email = form.Email,
            FullName = form.FullName
        };

        var result = await _userManager.CreateAsync(user, form.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View("Login", new AuthPageViewModel { ActiveTab = "register", Register = form });
        }

        await _demoDataSeeder.SeedForNewUserAsync(user.Id);
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Plans");
    }
}
