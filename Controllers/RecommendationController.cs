using Microsoft.AspNetCore.Mvc;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

public class RecommendationController : Controller
{
    private readonly ProductRecommendationService _productRecommendationService;

    public RecommendationController(ProductRecommendationService productRecommendationService)
    {
        _productRecommendationService = productRecommendationService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = _productRecommendationService.GetTrendingRecommendations();
        return View(model);
    }
}