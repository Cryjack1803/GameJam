using Microsoft.AspNetCore.Mvc;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

public class ForecastController : Controller
{
    private readonly SalesForecastService _salesForecastService;

    public ForecastController(SalesForecastService salesForecastService)
    {
        _salesForecastService = salesForecastService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = _salesForecastService.BuildForecast();
        return View(model);
    }
}