using System.Text.Json;
using Tarea_01.Models;

namespace Tarea_01.Services;

public class CartSessionService
{
    private const string SessionKey = "Cart.Items";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<CartItemViewModel> GetItems()
    {
        var session = _httpContextAccessor.HttpContext?.Session;

        if (session is null)
        {
            return new List<CartItemViewModel>();
        }

        var data = session.GetString(SessionKey);
        return string.IsNullOrWhiteSpace(data)
            ? new List<CartItemViewModel>()
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(data) ?? new List<CartItemViewModel>();
    }

    public void Add(ProductViewModel product, int quantity = 1)
    {
        var items = GetItems();
        var existingItem = items.FirstOrDefault(item => item.ProductId == product.Id);

        if (existingItem is null)
        {
            items.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }
        else
        {
            existingItem.Quantity += quantity;
        }

        Save(items);
    }

    public void Remove(int productId)
    {
        var items = GetItems();
        var item = items.FirstOrDefault(entry => entry.ProductId == productId);

        if (item is null)
        {
            return;
        }

        items.Remove(item);
        Save(items);
    }

    public void Clear()
    {
        Save(new List<CartItemViewModel>());
    }

    private void Save(List<CartItemViewModel> items)
    {
        _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }
}