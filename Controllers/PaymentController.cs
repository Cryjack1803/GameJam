using System.Text.Json;
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

        pendingPayment.PreferenceId = preferenceResult.PreferenceId;
        _paymentSessionService.Save(pendingPayment);
        return Redirect(preferenceResult.CheckoutUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> MercadoPagoSuccess(long? payment_id, long? collection_id, string? external_reference, CancellationToken cancellationToken)
    {
        var mercadoPagoPaymentId = payment_id ?? collection_id;

        if (!mercadoPagoPaymentId.HasValue)
        {
            TempData["CartMessage"] = "Mercado Pago no devolvio un identificador de pago para validar la compra.";
            return RedirectToAction("Index", "Cart");
        }

        var verificationResult = await _mercadoPagoPaymentService.VerifyApprovedPaymentAsync(mercadoPagoPaymentId.Value, cancellationToken);

        if (!verificationResult.Succeeded)
        {
            TempData["CartMessage"] = verificationResult.ErrorMessage;
            return RedirectToAction("Index", "Cart");
        }

        var externalReference = string.IsNullOrWhiteSpace(external_reference)
            ? verificationResult.ExternalReference
            : external_reference;

        var pendingPayment = _paymentSessionService.GetByExternalReference(externalReference) ?? _paymentSessionService.Get();

        if (pendingPayment is null ||
            !string.Equals(verificationResult.ExternalReference, pendingPayment.ExternalReference, StringComparison.OrdinalIgnoreCase))
        {
            TempData["CartMessage"] = "La referencia del pago aprobado no coincide con la compra pendiente.";
            return RedirectToAction("Index", "Cart");
        }

        if (!string.IsNullOrWhiteSpace(pendingPayment.PreferenceId) &&
            !string.Equals(verificationResult.PreferenceId, pendingPayment.PreferenceId, StringComparison.OrdinalIgnoreCase))
        {
            TempData["CartMessage"] = "La preferencia confirmada por Mercado Pago no coincide con la compra pendiente.";
            return RedirectToAction("Index", "Cart");
        }

        if (!_paymentSessionService.TryBeginCompletion(pendingPayment.ExternalReference, out var lockedPayment))
        {
            _cartSessionService.Clear();
            _paymentSessionService.Clear();
            TempData["CartMessage"] = "El pago ya fue procesado previamente.";
            return RedirectToAction("Index", "Cart");
        }

        if (!_salesCheckoutService.TryCheckout(lockedPayment!.BuyerName, lockedPayment.Items, out _, out var errorMessage))
        {
            _paymentSessionService.ReleaseCompletion(pendingPayment.ExternalReference);
            TempData["CartMessage"] = errorMessage;
            return RedirectToAction("Index", "Cart");
        }

        _cartSessionService.Clear();
        _paymentSessionService.Complete(pendingPayment.ExternalReference);
        TempData["CartMessage"] = "Pago aprobado en Mercado Pago y venta registrada correctamente.";
        return RedirectToAction("Index", "Cart");
    }

    [AllowAnonymous]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> MercadoPagoWebhook([FromBody] JsonElement payload, [FromQuery] string? type, [FromQuery] string? topic, [FromQuery] long? id, CancellationToken cancellationToken)
    {
        var notificationType = type ?? topic ?? ReadString(payload, "type") ?? ReadString(payload, "topic");

        if (!string.Equals(notificationType, "payment", StringComparison.OrdinalIgnoreCase))
        {
            return Ok();
        }

        var paymentId = id ?? ReadLong(payload, "data", "id") ?? ReadLong(payload, "id");

        if (!paymentId.HasValue)
        {
            return Ok();
        }

        var verificationResult = await _mercadoPagoPaymentService.VerifyApprovedPaymentAsync(paymentId.Value, cancellationToken);

        if (!verificationResult.Succeeded)
        {
            return Ok();
        }

        var pendingPayment = _paymentSessionService.GetByExternalReference(verificationResult.ExternalReference)
            ?? _paymentSessionService.GetByPreferenceId(verificationResult.PreferenceId);

        if (pendingPayment is null ||
            !_paymentSessionService.TryBeginCompletion(pendingPayment.ExternalReference, out var lockedPayment))
        {
            return Ok();
        }

        if (!_salesCheckoutService.TryCheckout(lockedPayment!.BuyerName, lockedPayment.Items, out _, out _))
        {
            _paymentSessionService.ReleaseCompletion(pendingPayment.ExternalReference);
            return Ok();
        }

        _paymentSessionService.Complete(pendingPayment.ExternalReference);
        return Ok();
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

    private static string? ReadString(JsonElement payload, string propertyName)
    {
        return payload.ValueKind == JsonValueKind.Object &&
               payload.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static long? ReadLong(JsonElement payload, string parentPropertyName, string propertyName)
    {
        if (payload.ValueKind != JsonValueKind.Object ||
            !payload.TryGetProperty(parentPropertyName, out var parent) ||
            parent.ValueKind != JsonValueKind.Object ||
            !parent.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt64(out var value) => value,
            JsonValueKind.String when long.TryParse(property.GetString(), out var value) => value,
            _ => null
        };
    }

    private static long? ReadLong(JsonElement payload, string propertyName)
    {
        if (payload.ValueKind != JsonValueKind.Object ||
            !payload.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt64(out var value) => value,
            JsonValueKind.String when long.TryParse(property.GetString(), out var value) => value,
            _ => null
        };
    }
}