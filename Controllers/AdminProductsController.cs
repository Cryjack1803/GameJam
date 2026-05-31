using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminProductsController : Controller
{
    private readonly ProductInventoryStore _inventoryStore;

    public AdminProductsController(ProductInventoryStore inventoryStore)
    {
        _inventoryStore = inventoryStore;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var products = _inventoryStore.GetAll();
        return View(products);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new ProductViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        _inventoryStore.Create(model);
        TempData["AdminProductMessage"] = "Producto registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _inventoryStore.GetById(id);

        if (product is null)
        {
            return NotFound();
        }

        return View("Form", product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        if (!_inventoryStore.Update(model))
        {
            return NotFound();
        }

        TempData["AdminProductMessage"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (!_inventoryStore.Delete(id))
        {
            return NotFound();
        }

        TempData["AdminProductMessage"] = "Producto eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}