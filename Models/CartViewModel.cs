namespace Tarea_01.Models;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    public List<SaleRecordViewModel> RecentSales { get; set; } = new();

    public decimal Total => Items.Sum(item => item.Subtotal);
}