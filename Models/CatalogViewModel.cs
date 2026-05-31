namespace Tarea_01.Models;

public class CatalogViewModel
{
    public string? Search { get; set; }

    public string? Category { get; set; }

    public List<string> Categories { get; set; } = new();

    public List<ProductViewModel> Products { get; set; } = new();
}