using Tarea_01.Models;

namespace Tarea_01.Services;

public class SalesHistoryStore
{
    private readonly List<SaleRecordViewModel> _sales = new();

    public SalesHistoryStore()
    {
        SeedHistory();
    }

    public IReadOnlyList<SaleRecordViewModel> GetAll()
    {
        return _sales.OrderByDescending(sale => sale.CreatedAt).ToList();
    }

    public IReadOnlyList<SaleRecordViewModel> GetRecent(int take = 5)
    {
        return _sales.OrderByDescending(sale => sale.CreatedAt).Take(take).ToList();
    }

    public void Add(SaleRecordViewModel sale)
    {
        sale.Id = _sales.Count == 0 ? 1 : _sales.Max(item => item.Id) + 1;
        _sales.Add(sale);
    }

    private void SeedHistory()
    {
        if (_sales.Count > 0)
        {
            return;
        }

        var seedTotals = new decimal[]
        {
            182m, 195m, 176m, 204m, 221m, 248m, 264m,
            190m, 202m, 187m, 214m, 228m, 252m, 271m
        };

        var productNames = new[] { "Arroz Superior", "Leche Entera", "Gaseosa Cola 3L" };

        for (var index = 0; index < seedTotals.Length; index++)
        {
            _sales.Add(new SaleRecordViewModel
            {
                Id = index + 1,
                BuyerName = $"Cliente Demo {index + 1}",
                Total = seedTotals[index],
                CreatedAt = DateTime.Today.AddDays(-(seedTotals.Length - index)),
                Items = new List<CartItemViewModel>
                {
                    new() { ProductId = 1, ProductName = productNames[0], Quantity = 3 + (index % 2), UnitPrice = 5.90m },
                    new() { ProductId = 2, ProductName = productNames[1], Quantity = 2 + (index % 3), UnitPrice = 4.20m },
                    new() { ProductId = 4, ProductName = productNames[2], Quantity = 4 + (index % 2), UnitPrice = 11.90m }
                }
            });
        }
    }
}