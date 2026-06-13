using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

public class AgentController : Controller
{
    private readonly AgentService _agentService;

    public AgentController(AgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new AgentViewModel
        {
            Response = "Bienvenido al asistente IA del minimarket. Pregunta por inventario, ventas, recomendaciones o promociones."
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Ask(AgentViewModel model)
    {
        if (model is null)
        {
            return RedirectToAction(nameof(Index));
        }

        model.Response = _agentService.Answer(model.Query, User.Identity?.Name ?? "Gerente");
        return View("Index", model);
    }
}
