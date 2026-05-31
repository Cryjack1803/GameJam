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
    private readonly SalesHistoryStore _salesHistoryStore;
    private readonly SalesCheckoutService _salesCheckoutService;

    public CartController(
        CartSessionService cartSessionService,
        SalesHistoryStore salesHistoryStore,
        SalesCheckoutService salesCheckoutService)
    {
        _cartSessionService = cartSessionService;
        _salesHistoryStore = salesHistoryStore;
        _salesCheckoutService = salesCheckoutService;
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
        if (_salesCheckoutService.TryAddToCart(productId, _cartSessionService.GetItems(), out var product, out var errorMessage))
        {
            _cartSessionService.Add(product!);
            TempData["CartMessage"] = "Producto agregado al carrito.";
            return RedirectToAction(nameof(Index));
        }

        TempData["CartMessage"] = errorMessage;
        return RedirectToAction("Index", "Catalog");
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

        var buyerName = User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity?.Name ?? "Cliente";

        if (!_salesCheckoutService.TryCheckout(buyerName, items, out var _, out var errorMessage))
        {
            TempData["CartMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        _cartSessionService.Clear();
        TempData["CartMessage"] = "Venta registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}