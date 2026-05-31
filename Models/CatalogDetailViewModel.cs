namespace Tarea_01.Models;

public class CatalogDetailViewModel
{
    public ProductViewModel Product { get; set; } = new();

    public List<ProductRecommendationViewModel> Recommendations { get; set; } = new();
}