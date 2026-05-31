namespace Tarea_01.Models;

public class ProductRecommendationViewModel
{
    public ProductViewModel Product { get; set; } = new();

    public string Reason { get; set; } = string.Empty;

    public decimal Score { get; set; }
}