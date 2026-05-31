using System.ComponentModel.DataAnnotations;

namespace Tarea_01.Models;

public class ProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingresa el nombre del producto.")]
    [StringLength(80, ErrorMessage = "El nombre no debe superar los 80 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa la categoria.")]
    [StringLength(40, ErrorMessage = "La categoria no debe superar los 40 caracteres.")]
    public string Category { get; set; } = string.Empty;

    [Range(0.1, 9999, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Price { get; set; }

    [Range(0, 9999, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Ingresa la unidad de venta.")]
    [StringLength(40, ErrorMessage = "La unidad no debe superar los 40 caracteres.")]
    public string Unit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa la descripcion del producto.")]
    [StringLength(220, ErrorMessage = "La descripcion no debe superar los 220 caracteres.")]
    public string Description { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
}