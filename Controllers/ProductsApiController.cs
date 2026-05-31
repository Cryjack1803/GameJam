using Microsoft.AspNetCore.Mvc;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ProductInventoryStore _inventoryStore;
    private readonly ProductRecommendationService _recommendationService;

    public ProductsApiController(ProductInventoryStore inventoryStore, ProductRecommendationService recommendationService)
    {
        _inventoryStore = inventoryStore;
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_inventoryStore.GetAll());
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var product = _inventoryStore.GetById(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("{id:int}/recommendations")]
    public IActionResult GetRecommendations(int id)
    {
        if (_inventoryStore.GetById(id) is null)
        {
            return NotFound();
        }

        return Ok(_recommendationService.GetRecommendationsForProduct(id));
    }
}