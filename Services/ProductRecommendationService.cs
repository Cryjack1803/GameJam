using Tarea_01.Models;

namespace Tarea_01.Services;

public class ProductRecommendationService
{
    private readonly ProductInventoryStore _inventoryStore;
    private readonly SalesHistoryStore _salesHistoryStore;

    public ProductRecommendationService(ProductInventoryStore inventoryStore, SalesHistoryStore salesHistoryStore)
    {
        _inventoryStore = inventoryStore;
        _salesHistoryStore = salesHistoryStore;
    }

    public List<ProductRecommendationViewModel> GetRecommendationsForProduct(int productId, int take = 3)
    {
        var targetProduct = _inventoryStore.GetById(productId);

        if (targetProduct is null)
        {
            return new List<ProductRecommendationViewModel>();
        }

        var products = _inventoryStore.GetAll();
        var coPurchaseScores = _salesHistoryStore.GetAll()
            .Where(sale => sale.Items.Any(item => item.ProductId == productId))
            .SelectMany(sale => sale.Items.Where(item => item.ProductId != productId))
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

        return products
            .Where(product => product.Id != productId)
            .Select(product =>
            {
                var score = 0m;
                var reason = "Relacion por categoria";

                if (coPurchaseScores.TryGetValue(product.Id, out var coPurchaseScore))
                {
                    score += coPurchaseScore * 2m;
                    reason = "Comprados juntos";
                }

                if (product.Category == targetProduct.Category)
                {
                    score += 1.5m;
                }

                if (product.IsFeatured)
                {
                    score += 0.5m;
                }

                return new ProductRecommendationViewModel
                {
                    Product = product,
                    Score = decimal.Round(score, 1),
                    Reason = reason
                };
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Product.Name)
            .Take(take)
            .ToList();
    }

    public List<ProductRecommendationViewModel> GetTrendingRecommendations(int take = 6)
    {
        var salesScores = _salesHistoryStore.GetAll()
            .SelectMany(sale => sale.Items)
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

        return _inventoryStore.GetAll()
            .Select(product => new ProductRecommendationViewModel
            {
                Product = product,
                Score = decimal.Round((salesScores.TryGetValue(product.Id, out var score) ? score : 0) + (product.IsFeatured ? 1m : 0m), 1),
                Reason = salesScores.ContainsKey(product.Id) ? "Alta rotacion" : "Sugerido por categoria"
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Product.Name)
            .Take(take)
            .ToList();
    }
}