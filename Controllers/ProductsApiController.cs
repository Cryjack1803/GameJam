using Microsoft.AspNetCore.Mvc;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ProductInventoryStore _inventoryStore;

    public ProductsApiController(ProductInventoryStore inventoryStore)
    {
        _inventoryStore = inventoryStore;
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
}