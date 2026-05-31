using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly CartSessionService _cartSessionService;
    private readonly ProductInventoryStore _inventoryStore;
    private readonly SalesHistoryStore _salesHistoryStore;

    public CartController(
        CartSessionService cartSessionService,
        ProductInventoryStore inventoryStore,
        SalesHistoryStore salesHistoryStore)
    {
        _cartSessionService = cartSessionService;
        _inventoryStore = inventoryStore;
        _salesHistoryStore = salesHistoryStore;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new CartViewModel
        {
            Items = _cartSessionService.GetItems(),
            RecentSales = _salesHistoryStore.GetRecent().ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(int productId)
    {
        var product = _inventoryStore.GetById(productId);

        if (product is null || product.Stock <= 0)
        {
            TempData["CartMessage"] = "El producto no esta disponible.";
            return RedirectToAction("Index", "Catalog");
        }

        var existingQuantity = _cartSessionService.GetItems()
            .Where(item => item.ProductId == productId)
            .Select(item => item.Quantity)
            .FirstOrDefault();

        if (!_inventoryStore.HasAvailableStock(productId, existingQuantity + 1))
        {
            TempData["CartMessage"] = "No hay mas stock disponible para agregar este producto.";
            return RedirectToAction(nameof(Index));
        }

        _cartSessionService.Add(product);
        TempData["CartMessage"] = "Producto agregado al carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cartSessionService.Remove(productId);
        TempData["CartMessage"] = "Producto retirado del carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout()
    {
        var items = _cartSessionService.GetItems();

        if (!items.Any())
        {
            TempData["CartMessage"] = "El carrito esta vacio.";
            return RedirectToAction(nameof(Index));
        }

        if (items.Any(item => !_inventoryStore.HasAvailableStock(item.ProductId, item.Quantity)))
        {
            TempData["CartMessage"] = "No hay stock suficiente para completar la compra.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var item in items)
        {
            _inventoryStore.TryDecreaseStock(item.ProductId, item.Quantity);
        }

        _salesHistoryStore.Add(new SaleRecordViewModel
        {
            BuyerName = User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity?.Name ?? "Cliente",
            Items = items.Select(item => new CartItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList(),
            Total = items.Sum(item => item.Subtotal),
            CreatedAt = DateTime.Now
        });

        _cartSessionService.Clear();
        TempData["CartMessage"] = "Venta registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}