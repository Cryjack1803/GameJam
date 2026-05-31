using Microsoft.AspNetCore.Mvc;
using Tarea_01.Models;

namespace Tarea_01.Controllers;

public class CatalogController : Controller
{
    private static readonly IReadOnlyList<ProductViewModel> Products = new List<ProductViewModel>
    {
        new() { Id = 1, Name = "Arroz Superior", Category = "Abarrotes", Price = 5.90m, Stock = 42, Unit = "bolsa 1kg", Description = "Producto basico para consumo diario y alta rotacion en el minimarket.", IsFeatured = true },
        new() { Id = 2, Name = "Leche Entera", Category = "Lacteos", Price = 4.20m, Stock = 18, Unit = "tarro 400g", Description = "Leche ideal para desayunos y preparaciones familiares.", IsFeatured = true },
        new() { Id = 3, Name = "Pan Molde Integral", Category = "Panaderia", Price = 7.50m, Stock = 10, Unit = "paquete", Description = "Opcion saludable para clientes frecuentes del turno manana.", IsFeatured = false },
        new() { Id = 4, Name = "Gaseosa Cola 3L", Category = "Bebidas", Price = 11.90m, Stock = 25, Unit = "botella", Description = "Producto de alto movimiento en fines de semana y promociones familiares.", IsFeatured = true },
        new() { Id = 5, Name = "Detergente Liquido", Category = "Limpieza", Price = 14.80m, Stock = 8, Unit = "botella 900ml", Description = "Insumo de limpieza con margen atractivo y frecuencia de recompra.", IsFeatured = false },
        new() { Id = 6, Name = "Papel Higienico x4", Category = "Hogar", Price = 9.60m, Stock = 6, Unit = "pack", Description = "Producto esencial con alta sensibilidad a descuentos y packs.", IsFeatured = false }
    };

    [HttpGet]
    public IActionResult Index(string? search, string? category)
    {
        var filteredProducts = Products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                product.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        var model = new CatalogViewModel
        {
            Search = search,
            Category = category,
            Categories = Products.Select(product => product.Category).Distinct().OrderBy(name => name).ToList(),
            Products = filteredProducts.OrderBy(product => product.Name).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Detail(int id)
    {
        var product = Products.FirstOrDefault(item => item.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }
}