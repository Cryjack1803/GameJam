using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;
using Tarea_01.Services;

namespace Tarea_01.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly CartSessionService _cartSessionService;
    private readonly MercadoPagoPaymentService _mercadoPagoPaymentService;
    private readonly PaymentSessionService _paymentSessionService;
    private readonly SalesCheckoutService _salesCheckoutService;

    public PaymentController(
        CartSessionService cartSessionService,
        MercadoPagoPaymentService mercadoPagoPaymentService,
        PaymentSessionService paymentSessionService,
        SalesCheckoutService salesCheckoutService)
    {
        _cartSessionService = cartSessionService;
        _mercadoPagoPaymentService = mercadoPagoPaymentService;
        _paymentSessionService = paymentSessionService;
        _salesCheckoutService = salesCheckoutService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartMercadoPago(CancellationToken cancellationToken)
    {
        var items = _cartSessionService.GetItems();

        if (!items.Any())
        {
            TempData["CartMessage"] = "El carrito esta vacio.";
            return RedirectToAction("Index", "Cart");
        }

        var pendingPayment = new PendingPaymentSessionViewModel
        {
            ExternalReference = Guid.NewGuid().ToString("N"),
            BuyerName = User.FindFirstValue(ClaimTypes.GivenName) ?? User.Identity?.Name ?? "Cliente",
            Items = items.Select(item => new CartItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };

        var preferenceResult = await _mercadoPagoPaymentService.CreatePreferenceAsync(pendingPayment, cancellationToken);

        if (!preferenceResult.Succeeded)
        {
            TempData["CartMessage"] = preferenceResult.ErrorMessage;
            return RedirectToAction("Index", "Cart");
        }

        _paymentSessionService.Save(pendingPayment);
        return Redirect(preferenceResult.CheckoutUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult MercadoPagoSuccess(string? external_reference)
    {
        var pendingPayment = _paymentSessionService.Get();

        if (pendingPayment is null ||
            !string.Equals(pendingPayment.ExternalReference, external_reference, StringComparison.OrdinalIgnoreCase))
        {
            TempData["CartMessage"] = "No se encontro una compra pendiente para confirmar.";
            return RedirectToAction("Index", "Cart");
        }

        if (!_salesCheckoutService.TryCheckout(pendingPayment.BuyerName, pendingPayment.Items, out _, out var errorMessage))
        {
            TempData["CartMessage"] = errorMessage;
            return RedirectToAction("Index", "Cart");
        }

        _cartSessionService.Clear();
        _paymentSessionService.Clear();
        TempData["CartMessage"] = "Pago aprobado en Mercado Pago y venta registrada correctamente.";
        return RedirectToAction("Index", "Cart");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult MercadoPagoFailure()
    {
        TempData["CartMessage"] = "El pago fue cancelado o rechazado en Mercado Pago.";
        return RedirectToAction("Index", "Cart");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult MercadoPagoPending()
    {
        TempData["CartMessage"] = "El pago quedo pendiente en Mercado Pago. Revisa el estado antes de cerrar la venta.";
        return RedirectToAction("Index", "Cart");
    }
}