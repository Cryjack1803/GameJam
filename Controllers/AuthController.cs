using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;

namespace Tarea_01.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            TempData["AuthMessage"] = "Ya existe una sesion activa en el sistema.";
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales no validas.");
            return View(model);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Credenciales no validas.");
            return View(model);
        }

        await _signInManager.SignOutAsync();

        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim> { new(ClaimTypes.GivenName, user.FullName) };

        if (userRoles.Count > 0)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRoles[0]));
        }

        await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);
        TempData["AuthMessage"] = $"Bienvenido, {user.FullName}.";

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Cliente");
        await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, new[]
        {
            new Claim(ClaimTypes.GivenName, user.FullName),
            new Claim(ClaimTypes.Role, "Cliente")
        });

        TempData["AuthMessage"] = "Tu cuenta fue creada correctamente.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["AuthMessage"] = "La sesion se cerro correctamente.";
        return RedirectToAction("Index", "Home");
    }
}