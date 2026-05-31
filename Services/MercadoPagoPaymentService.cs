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

    private sealed class MercadoPagoPreferenceRequest
    {
        [JsonPropertyName("items")]
        public List<MercadoPagoPreferenceItem> Items { get; set; } = new();

        [JsonPropertyName("back_urls")]
        public MercadoPagoBackUrls BackUrls { get; set; } = new();

        [JsonPropertyName("auto_return")]
        public string AutoReturn { get; set; } = "approved";

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
}