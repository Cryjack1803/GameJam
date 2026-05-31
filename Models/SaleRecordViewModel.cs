namespace Tarea_01.Models;

public class SaleRecordViewModel
{
    public int Id { get; set; }

    public string BuyerName { get; set; } = string.Empty;

    public List<CartItemViewModel> Items { get; set; } = new();

    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; }
}