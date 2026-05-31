using System.Collections.Concurrent;
using System.Text.Json;
using Tarea_01.Models;

namespace Tarea_01.Services;

public class PaymentSessionService
{
    private const string SessionKey = "Payment.PendingMercadoPago";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentDictionary<string, PendingPaymentSessionViewModel> _pendingPayments = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processingPayments = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    public PaymentSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Save(PendingPaymentSessionViewModel pendingPayment)
    {
        _pendingPayments[pendingPayment.ExternalReference] = Clone(pendingPayment);
        _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, pendingPayment.ExternalReference);
    }

    public PendingPaymentSessionViewModel? Get()
    {
        var rawValue = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (rawValue.StartsWith('{'))
        {
            var legacyPendingPayment = JsonSerializer.Deserialize<PendingPaymentSessionViewModel>(rawValue);

            if (legacyPendingPayment is not null)
            {
                Save(legacyPendingPayment);
            }

            return legacyPendingPayment;
        }

        return GetByExternalReference(rawValue);
    }

    public PendingPaymentSessionViewModel? GetByExternalReference(string? externalReference)
    {
        if (string.IsNullOrWhiteSpace(externalReference))
        {
            return null;
        }

        return _pendingPayments.TryGetValue(externalReference, out var pendingPayment)
            ? Clone(pendingPayment)
            : null;
    }

    public PendingPaymentSessionViewModel? GetByPreferenceId(string? preferenceId)
    {
        if (string.IsNullOrWhiteSpace(preferenceId))
        {
            return null;
        }

        var pendingPayment = _pendingPayments.Values.FirstOrDefault(item =>
            string.Equals(item.PreferenceId, preferenceId, StringComparison.OrdinalIgnoreCase));

        return pendingPayment is null ? null : Clone(pendingPayment);
    }

    public bool TryBeginCompletion(string externalReference, out PendingPaymentSessionViewModel? pendingPayment)
    {
        pendingPayment = null;

        lock (_gate)
        {
            if (!_pendingPayments.TryGetValue(externalReference, out var storedPayment) ||
                _processingPayments.Contains(externalReference))
            {
                return false;
            }

            _processingPayments.Add(externalReference);
            pendingPayment = Clone(storedPayment);
            return true;
        }
    }

    public void ReleaseCompletion(string externalReference)
    {
        lock (_gate)
        {
            _processingPayments.Remove(externalReference);
        }
    }

    public void Clear()
    {
        var externalReference = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);

        if (!string.IsNullOrWhiteSpace(externalReference))
        {
            RemovePending(externalReference);
        }

        _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
    }

    public void Complete(string externalReference)
    {
        RemovePending(externalReference);

        var currentReference = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);

        if (string.Equals(currentReference, externalReference, StringComparison.OrdinalIgnoreCase))
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }
    }

    private void RemovePending(string externalReference)
    {
        lock (_gate)
        {
            _processingPayments.Remove(externalReference);
            _pendingPayments.TryRemove(externalReference, out _);
        }
    }

    private static PendingPaymentSessionViewModel Clone(PendingPaymentSessionViewModel pendingPayment)
    {
        return new PendingPaymentSessionViewModel
        {
            ExternalReference = pendingPayment.ExternalReference,
            PreferenceId = pendingPayment.PreferenceId,
            BuyerName = pendingPayment.BuyerName,
            Items = pendingPayment.Items.Select(item => new CartItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }
}