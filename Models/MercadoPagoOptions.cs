namespace Tarea_01.Models;

public class MercadoPagoOptions
{
    public string AccessToken { get; set; } = string.Empty;

    public string AppBaseUrl { get; set; } = string.Empty;

    public bool UseSandbox { get; set; } = true;

    public string Currency { get; set; } = "PEN";
}