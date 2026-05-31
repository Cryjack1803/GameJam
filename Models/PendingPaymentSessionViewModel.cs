namespace Tarea_01.Models;

public class PendingPaymentSessionViewModel
{
    public string ExternalReference { get; set; } = string.Empty;

    public string PreferenceId { get; set; } = string.Empty;

    public string BuyerName { get; set; } = string.Empty;

    public List<CartItemViewModel> Items { get; set; } = new();
}