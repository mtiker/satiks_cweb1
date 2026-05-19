using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using App.Domain.Identity;
using WebApp.Models;

namespace WebApp.Areas.User.Controllers;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[Area("User")]
public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", model.Email);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsInRoleAsync(user, "admin"))
                    return RedirectToAction("Index", "Home", new { area = "Admin" });

                if (user != null && await _userManager.IsInRoleAsync(user, "technician"))
                    return RedirectToAction("Index", "TechnicianHome", new { area = "Technician" });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account {Email} locked out.", model.Email);
                ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created a new account with password.", model.Email);

                await _userManager.AddToRoleAsync(user, "user");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction(nameof(Login));
    }
}
