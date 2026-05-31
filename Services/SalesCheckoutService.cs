using Tarea_01.Models;

namespace Tarea_01.Services;

public class SalesCheckoutService
{
    private readonly ProductInventoryStore _inventoryStore;
    private readonly SalesHistoryStore _salesHistoryStore;

    public SalesCheckoutService(ProductInventoryStore inventoryStore, SalesHistoryStore salesHistoryStore)
    {
        _inventoryStore = inventoryStore;
        _salesHistoryStore = salesHistoryStore;
    }

    public bool TryAddToCart(int productId, IReadOnlyCollection<CartItemViewModel> currentItems, out ProductViewModel? product, out string errorMessage)
    {
        product = _inventoryStore.GetById(productId);

        if (product is null || product.Stock <= 0)
        {
            errorMessage = "El producto no esta disponible.";
            return false;
        }

        var existingQuantity = currentItems
            .Where(item => item.ProductId == productId)
            .Select(item => item.Quantity)
            .FirstOrDefault();

        if (!_inventoryStore.HasAvailableStock(productId, existingQuantity + 1))
        {
            errorMessage = "No hay mas stock disponible para agregar este producto.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public bool TryCheckout(string buyerName, IReadOnlyCollection<CartItemViewModel> items, out SaleRecordViewModel? sale, out string errorMessage)
    {
        sale = null;

        if (!items.Any())
        {
            errorMessage = "El carrito esta vacio.";
            return false;
        }

        if (items.Any(item => !_inventoryStore.HasAvailableStock(item.ProductId, item.Quantity)))
        {
            errorMessage = "No hay stock suficiente para completar la compra.";
            return false;
        }

        foreach (var item in items)
        {
            _inventoryStore.TryDecreaseStock(item.ProductId, item.Quantity);
        }

        sale = new SaleRecordViewModel
        {
            BuyerName = buyerName,
            Items = items.Select(item => new CartItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList(),
            Total = items.Sum(item => item.Subtotal),
            CreatedAt = DateTime.Now
        };

        _salesHistoryStore.Add(sale);
        errorMessage = string.Empty;
        return true;
    }
}