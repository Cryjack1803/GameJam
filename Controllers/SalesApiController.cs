using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesApiController : ControllerBase
{
    private readonly ProductInventoryStore _inventoryStore;
    private readonly SalesHistoryStore _salesHistoryStore;
    private readonly SalesCheckoutService _salesCheckoutService;

    public SalesApiController(
        ProductInventoryStore inventoryStore,
        SalesHistoryStore salesHistoryStore,
        SalesCheckoutService salesCheckoutService)
    {
        _inventoryStore = inventoryStore;
        _salesHistoryStore = salesHistoryStore;
        _salesCheckoutService = salesCheckoutService;
    }

    [HttpGet]
    public IActionResult GetRecent()
    {
        return Ok(_salesHistoryStore.GetAll());
    }

    [HttpPost]
    public IActionResult Create([FromBody] ApiSaleCheckoutRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var items = new List<CartItemViewModel>();

        foreach (var item in request.Items)
        {
            var product = _inventoryStore.GetById(item.ProductId);

            if (product is null)
            {
                return NotFound(new { message = $"No existe el producto {item.ProductId}." });
            }

            items.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity
            });
        }

        if (!_salesCheckoutService.TryCheckout(request.BuyerName, items, out var sale, out var errorMessage))
        {
            return BadRequest(new { message = errorMessage });
        }

        return CreatedAtAction(nameof(GetRecent), new { id = sale!.Id }, sale);
    }
}