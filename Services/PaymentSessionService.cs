using System.Text.Json;
using Tarea_01.Models;

namespace Tarea_01.Services;

public class PaymentSessionService
{
    private const string SessionKey = "Payment.PendingMercadoPago";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Save(PendingPaymentSessionViewModel pendingPayment)
    {
        _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, JsonSerializer.Serialize(pendingPayment));
    }

    public PendingPaymentSessionViewModel? Get()
    {
        var rawValue = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);

        return string.IsNullOrWhiteSpace(rawValue)
            ? null
            : JsonSerializer.Deserialize<PendingPaymentSessionViewModel>(rawValue);
    }

    public void Clear()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
    }
}