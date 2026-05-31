using System.ComponentModel.DataAnnotations;

namespace Tarea_01.Models;

public class ApiSaleCheckoutItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}