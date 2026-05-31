namespace Tarea_01.Models;

public class ProductViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
}