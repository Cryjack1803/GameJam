using System.ComponentModel.DataAnnotations;

namespace Tarea_01.Models;

public class ApiSaleCheckoutRequest
{
    [Required(ErrorMessage = "Ingresa el comprador.")]
    public string BuyerName { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Debes enviar al menos un item.")]
    public List<ApiSaleCheckoutItemRequest> Items { get; set; } = new();
}