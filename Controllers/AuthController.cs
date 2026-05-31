using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;

namespace Tarea_01.Controllers;

public class AuthController : Controller
{
    private const string SessionUserName = "Auth.UserName";
    private const string SessionUserRole = "Auth.UserRole";

    private static readonly List<DemoUser> DemoUsers = new()
    {
        new DemoUser("admin@minimarket.com", "Admin123*", "Administrador", "Cristopher"),
        new DemoUser("cliente@minimarket.com", "Cliente123*", "Cliente", "Cliente Demo"),
        new DemoUser("cajero@minimarket.com", "Cajero123*", "Cajero", "Caja Central")
    };

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString(SessionUserName) is not null)
        {
            TempData["AuthMessage"] = "Ya existe una sesion activa en el sistema.";
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var matchedUser = DemoUsers.FirstOrDefault(user =>
            user.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) &&
            user.Password == model.Password);

        if (matchedUser is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales no validas. Usa uno de los accesos de prueba.");
            return View(model);
        }

        HttpContext.Session.SetString(SessionUserName, matchedUser.DisplayName);
        HttpContext.Session.SetString(SessionUserRole, matchedUser.Role);
        TempData["AuthMessage"] = $"Bienvenido, {matchedUser.DisplayName}.";

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["AuthMessage"] = "La sesion se cerro correctamente.";
        return RedirectToAction("Index", "Home");
    }

    private sealed record DemoUser(string Email, string Password, string Role, string DisplayName);
}