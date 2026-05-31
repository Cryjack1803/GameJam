using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tarea_01.Models;

namespace Tarea_01.Services;

public class MercadoPagoPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly MercadoPagoOptions _options;

    public MercadoPagoPaymentService(HttpClient httpClient, IOptions<MercadoPagoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.AccessToken) &&
        !string.IsNullOrWhiteSpace(_options.AppBaseUrl);

    public async Task<MercadoPagoPreferenceResult> CreatePreferenceAsync(PendingPaymentSessionViewModel pendingPayment, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return MercadoPagoPreferenceResult.Fail("Mercado Pago no esta configurado. Define AccessToken y AppBaseUrl.");
        }

        var baseUrl = _options.AppBaseUrl.TrimEnd('/');
        var request = new MercadoPagoPreferenceRequest
        {
            Items = pendingPayment.Items.Select(item => new MercadoPagoPreferenceItem
            {
                Id = item.ProductId.ToString(),
                Title = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                CurrencyId = _options.Currency
            }).ToList(),
            BackUrls = new MercadoPagoBackUrls
            {
                Success = $"{baseUrl}/Payment/MercadoPagoSuccess",
                Failure = $"{baseUrl}/Payment/MercadoPagoFailure",
                Pending = $"{baseUrl}/Payment/MercadoPagoPending"
            },
            NotificationUrl = $"{baseUrl}/Payment/MercadoPagoWebhook",
            AutoReturn = "approved",
            ExternalReference = pendingPayment.ExternalReference,
            StatementDescriptor = "MINIMARKET"
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/checkout/preferences")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return MercadoPagoPreferenceResult.Fail($"Mercado Pago rechazo la preferencia: {responseBody}");
        }

        var preference = await response.Content.ReadFromJsonAsync<MercadoPagoPreferenceResponse>(cancellationToken: cancellationToken);

        if (preference is null)
        {
            return MercadoPagoPreferenceResult.Fail("Mercado Pago no devolvio una preferencia valida.");
        }

        var checkoutUrl = _options.UseSandbox && !string.IsNullOrWhiteSpace(preference.SandboxInitPoint)
            ? preference.SandboxInitPoint
            : preference.InitPoint;

        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return MercadoPagoPreferenceResult.Fail("Mercado Pago no devolvio una URL de checkout.");
        }

        return MercadoPagoPreferenceResult.Success(checkoutUrl, preference.Id);
    }

    public async Task<MercadoPagoPaymentVerificationResult> VerifyApprovedPaymentAsync(long paymentId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return MercadoPagoPaymentVerificationResult.Fail("Mercado Pago no esta configurado. Define AccessToken y AppBaseUrl.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Get, $"https://api.mercadopago.com/v1/payments/{paymentId}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return MercadoPagoPaymentVerificationResult.Fail($"No se pudo validar el pago en Mercado Pago: {responseBody}");
        }

        var payment = await response.Content.ReadFromJsonAsync<MercadoPagoPaymentResponse>(cancellationToken: cancellationToken);

        if (payment is null)
        {
            return MercadoPagoPaymentVerificationResult.Fail("Mercado Pago no devolvio un pago valido para confirmar la compra.");
        }

        if (!string.Equals(payment.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            var paymentStatus = string.IsNullOrWhiteSpace(payment.Status) ? "desconocido" : payment.Status;
            return MercadoPagoPaymentVerificationResult.Fail($"El pago en Mercado Pago todavia no esta aprobado. Estado actual: {paymentStatus}.");
        }

        return MercadoPagoPaymentVerificationResult.Success(payment.ExternalReference, payment.Order?.Id ?? string.Empty, payment.Status);
    }

    public sealed class MercadoPagoPreferenceResult
    {
        public bool Succeeded { get; private set; }

        public string CheckoutUrl { get; private set; } = string.Empty;

        public string PreferenceId { get; private set; } = string.Empty;

        public string ErrorMessage { get; private set; } = string.Empty;

        public static MercadoPagoPreferenceResult Success(string checkoutUrl, string preferenceId)
        {
            return new MercadoPagoPreferenceResult
            {
                Succeeded = true,
                CheckoutUrl = checkoutUrl,
                PreferenceId = preferenceId
            };
        }

        public static MercadoPagoPreferenceResult Fail(string errorMessage)
        {
            return new MercadoPagoPreferenceResult
            {
                Succeeded = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public sealed class MercadoPagoPaymentVerificationResult
    {
        public bool Succeeded { get; private set; }

        public string ExternalReference { get; private set; } = string.Empty;

        public string PreferenceId { get; private set; } = string.Empty;

        public string Status { get; private set; } = string.Empty;

        public string ErrorMessage { get; private set; } = string.Empty;

        public static MercadoPagoPaymentVerificationResult Success(string externalReference, string preferenceId, string status)
        {
            return new MercadoPagoPaymentVerificationResult
            {
                Succeeded = true,
                ExternalReference = externalReference,
                PreferenceId = preferenceId,
                Status = status
            };
        }

        public static MercadoPagoPaymentVerificationResult Fail(string errorMessage)
        {
            return new MercadoPagoPaymentVerificationResult
            {
                ErrorMessage = errorMessage
            };
        }
    }

    private sealed class MercadoPagoPreferenceRequest
    {
        [JsonPropertyName("items")]
        public List<MercadoPagoPreferenceItem> Items { get; set; } = new();

        [JsonPropertyName("back_urls")]
        public MercadoPagoBackUrls BackUrls { get; set; } = new();

        [JsonPropertyName("auto_return")]
        public string AutoReturn { get; set; } = "approved";

        [JsonPropertyName("notification_url")]
        public string NotificationUrl { get; set; } = string.Empty;

        [JsonPropertyName("external_reference")]
        public string ExternalReference { get; set; } = string.Empty;

        [JsonPropertyName("statement_descriptor")]
        public string StatementDescriptor { get; set; } = string.Empty;
    }

    private sealed class MercadoPagoPreferenceItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("currency_id")]
        public string CurrencyId { get; set; } = string.Empty;
    }

    private sealed class MercadoPagoBackUrls
    {
        [JsonPropertyName("success")]
        public string Success { get; set; } = string.Empty;

        [JsonPropertyName("failure")]
        public string Failure { get; set; } = string.Empty;

        [JsonPropertyName("pending")]
        public string Pending { get; set; } = string.Empty;
    }

    private sealed class MercadoPagoPreferenceResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("init_point")]
        public string InitPoint { get; set; } = string.Empty;

        [JsonPropertyName("sandbox_init_point")]
        public string SandboxInitPoint { get; set; } = string.Empty;
    }

    private sealed class MercadoPagoPaymentResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("external_reference")]
        public string ExternalReference { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public MercadoPagoPaymentOrder? Order { get; set; }
    }

    private sealed class MercadoPagoPaymentOrder
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }
}