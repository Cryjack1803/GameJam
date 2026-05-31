using Tarea_01.Models;

namespace Tarea_01.Services;

public class SalesHistoryStore
{
    private readonly List<SaleRecordViewModel> _sales = new();

    public IReadOnlyList<SaleRecordViewModel> GetRecent(int take = 5)
    {
        return _sales.OrderByDescending(sale => sale.CreatedAt).Take(take).ToList();
    }

    public void Add(SaleRecordViewModel sale)
    {
        sale.Id = _sales.Count == 0 ? 1 : _sales.Max(item => item.Id) + 1;
        _sales.Add(sale);
    }
}