using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

public class CatalogController : Controller
{
    private readonly ProductInventoryStore _inventoryStore;
    private readonly ProductRecommendationService _recommendationService;

    public CatalogController(ProductInventoryStore inventoryStore, ProductRecommendationService recommendationService)
    {
        _inventoryStore = inventoryStore;
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public IActionResult Index(string? search, string? category)
    {
        var products = _inventoryStore.GetAll();
        var filteredProducts = products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                product.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        var model = new CatalogViewModel
        {
            Search = search,
            Category = category,
            Categories = products.Select(product => product.Category).Distinct().OrderBy(name => name).ToList(),
            Products = filteredProducts.OrderBy(product => product.Name).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Detail(int id)
    {
        var product = _inventoryStore.GetById(id);

        if (product is null)
        {
            return NotFound();
        }

        var model = new CatalogDetailViewModel
        {
            Product = product,
            Recommendations = _recommendationService.GetRecommendationsForProduct(id)
        };

        return View(model);
    }
}