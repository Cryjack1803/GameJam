using Tarea_01.Models;

namespace Tarea_01.Services;

public class ProductInventoryStore
{
    private readonly List<ProductViewModel> _products =
    [
        new() { Id = 1, Name = "Arroz Superior", Category = "Abarrotes", Price = 5.90m, Stock = 42, Unit = "bolsa 1kg", Description = "Producto basico para consumo diario y alta rotacion en el minimarket.", IsFeatured = true },
        new() { Id = 2, Name = "Leche Entera", Category = "Lacteos", Price = 4.20m, Stock = 18, Unit = "tarro 400g", Description = "Leche ideal para desayunos y preparaciones familiares.", IsFeatured = true },
        new() { Id = 3, Name = "Pan Molde Integral", Category = "Panaderia", Price = 7.50m, Stock = 10, Unit = "paquete", Description = "Opcion saludable para clientes frecuentes del turno manana.", IsFeatured = false },
        new() { Id = 4, Name = "Gaseosa Cola 3L", Category = "Bebidas", Price = 11.90m, Stock = 25, Unit = "botella", Description = "Producto de alto movimiento en fines de semana y promociones familiares.", IsFeatured = true },
        new() { Id = 5, Name = "Detergente Liquido", Category = "Limpieza", Price = 14.80m, Stock = 8, Unit = "botella 900ml", Description = "Insumo de limpieza con margen atractivo y frecuencia de recompra.", IsFeatured = false },
        new() { Id = 6, Name = "Papel Higienico x4", Category = "Hogar", Price = 9.60m, Stock = 6, Unit = "pack", Description = "Producto esencial con alta sensibilidad a descuentos y packs.", IsFeatured = false }
    ];

    public List<ProductViewModel> GetAll()
    {
        return _products.OrderBy(product => product.Name).Select(Clone).ToList();
    }

    public ProductViewModel? GetById(int id)
    {
        var product = _products.FirstOrDefault(item => item.Id == id);
        return product is null ? null : Clone(product);
    }

    public void Create(ProductViewModel product)
    {
        var nextId = _products.Count == 0 ? 1 : _products.Max(item => item.Id) + 1;
        var storedProduct = Clone(product);
        storedProduct.Id = nextId;
        _products.Add(storedProduct);
    }

    public bool Update(ProductViewModel product)
    {
        var existingProduct = _products.FirstOrDefault(item => item.Id == product.Id);

        if (existingProduct is null)
        {
            return false;
        }

        existingProduct.Name = product.Name;
        existingProduct.Category = product.Category;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.Unit = product.Unit;
        existingProduct.Description = product.Description;
        existingProduct.IsFeatured = product.IsFeatured;

        return true;
    }

    public bool Delete(int id)
    {
        var existingProduct = _products.FirstOrDefault(item => item.Id == id);

        if (existingProduct is null)
        {
            return false;
        }

        _products.Remove(existingProduct);
        return true;
    }

    public bool HasAvailableStock(int id, int quantity)
    {
        var existingProduct = _products.FirstOrDefault(item => item.Id == id);
        return existingProduct is not null && existingProduct.Stock >= quantity;
    }

    public bool TryDecreaseStock(int id, int quantity)
    {
        var existingProduct = _products.FirstOrDefault(item => item.Id == id);

        if (existingProduct is null || existingProduct.Stock < quantity)
        {
            return false;
        }

        existingProduct.Stock -= quantity;
        return true;
    }

    private static ProductViewModel Clone(ProductViewModel product)
    {
        return new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock,
            Unit = product.Unit,
            Description = product.Description,
            IsFeatured = product.IsFeatured
        };
    }
}