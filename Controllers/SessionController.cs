using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;

namespace Tarea_01.Controllers;

[Authorize]
public class SessionController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var startedAtRaw = HttpContext.Session.GetString("Session.StartedAt");
        DateTime? startedAt = null;

        if (DateTime.TryParse(startedAtRaw, out var parsed))
        {
            startedAt = parsed.ToLocalTime();
        }

        var model = new SessionViewModel
        {
            UserName = User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity?.Name ?? "Usuario",
            Role = HttpContext.Session.GetString("Session.UserRole") ?? User.FindFirstValue(ClaimTypes.Role) ?? "Sin rol",
            StartedAt = startedAt,
            IsPersistent = Request.Cookies.ContainsKey(".AspNetCore.Identity.Application"),
            SessionId = HttpContext.Session.Id,
            AuthenticatedAt = startedAt?.ToString("dd/MM/yyyy HH:mm") ?? "No disponible"
        };

        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public IActionResult Admin()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}