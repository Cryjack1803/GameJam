using Tarea_01.Models;

namespace Tarea_01.Services;

public class AgentService
{
    private readonly ProductInventoryStore _inventoryStore;
    private readonly SalesHistoryStore _salesHistoryStore;
    private readonly SalesForecastService _salesForecastService;
    private readonly ProductRecommendationService _recommendationService;

    public AgentService(
        ProductInventoryStore inventoryStore,
        SalesHistoryStore salesHistoryStore,
        SalesForecastService salesForecastService,
        ProductRecommendationService recommendationService)
    {
        _inventoryStore = inventoryStore;
        _salesHistoryStore = salesHistoryStore;
        _salesForecastService = salesForecastService;
        _recommendationService = recommendationService;
    }

    public string Answer(string query, string userName = "Gerente")
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return $"Hola {userName}, soy el agente IA del minimarket. Pregunta sobre inventario, ventas, sugerencias o promociones y te doy un resumen comercial.";
        }

        var prompt = query.Trim().ToLowerInvariant();

        if (prompt.Contains("stock") || prompt.Contains("agotado") || prompt.Contains("reponer") || prompt.Contains("inventario"))
        {
            return BuildInventoryAdvice();
        }

        if (prompt.Contains("venta") || prompt.Contains("tendencia") || prompt.Contains("forecast") || prompt.Contains("pronóstico") || prompt.Contains("predic") )
        {
            return BuildSalesAdvice();
        }

        if (prompt.Contains("recomend") || prompt.Contains("suger") || prompt.Contains("mejor") || prompt.Contains("producto"))
        {
            return BuildRecommendationAdvice();
        }

        if (prompt.Contains("precio") || prompt.Contains("promoc") || prompt.Contains("oferta") || prompt.Contains("margen"))
        {
            return BuildPromotionAdvice();
        }

        return BuildExecutiveSummary();
    }

    private string BuildInventoryAdvice()
    {
        var lowStock = _inventoryStore.GetAll()
            .Where(p => p.Stock <= 10)
            .OrderBy(p => p.Stock)
            .ToList();

        if (!lowStock.Any())
        {
            return "El inventario se ve saludable. No hay productos críticos de baja existencia en este momento.";
        }

        var lines = lowStock.Select(p => $"- {p.Name} ({p.Category}): {p.Stock} unidades restantes.");
        return "Atención al inventario:\n" + string.Join("\n", lines) + "\nRecomiendo reponer los productos con stock bajo antes de la próxima promoción.";
    }

    private string BuildSalesAdvice()
    {
        var forecast = _salesForecastService.BuildForecast();
        var bestDay = forecast.ForecastPoints.OrderByDescending(p => p.PredictedTotal).First();
        var average = forecast.AverageDailySales;

        return $"El pronóstico de ventas muestra una media de S/ {average:0.00} diarios en la próxima semana. " +
               $"El día con mayor potencial estimado es {bestDay.Date:dddd d/M} con S/ {bestDay.PredictedTotal:0.00}. " +
               "Mantén inventario en las categorías de alta rotación y considera promociones para los días con menor demanda prevista.";
    }

    private string BuildRecommendationAdvice()
    {
        var recommendations = _recommendationService.GetTrendingRecommendations(4);

        if (!recommendations.Any())
        {
            return "No hay suficientes datos para generar recomendaciones en este momento.";
        }

        var lines = recommendations.Select(item => $"- {item.Product.Name}: {item.Reason} (puntaje {item.Score:0.0}).");
        return "Sugerencias de producto para impulsar venta:\n" + string.Join("\n", lines) +
               "\nPrioriza estos productos en exhibición y tarjeta de oferta.";
    }

    private string BuildPromotionAdvice()
    {
        var cheapest = _inventoryStore.GetAll().OrderBy(p => p.Price).FirstOrDefault();
        var featured = _inventoryStore.GetAll().Where(p => p.IsFeatured).Take(3).ToList();

        var lines = featured.Select(p => $"- {p.Name} ({p.Category}) a S/ {p.Price:0.00}");
        return "Para una promoción rápida te recomiendo: \n" +
               (cheapest is not null ? $"- Producto económico para oferta diaria: {cheapest.Name} a S/ {cheapest.Price:0.00}\n" : string.Empty) +
               string.Join("\n", lines) +
               "\nOfrece un combo entre productos de alta rotación y precio accesible.";
    }

    private string BuildExecutiveSummary()
    {
        var topProducts = _inventoryStore.GetAll().OrderByDescending(p => p.IsFeatured ? 1 : 0).ThenBy(p => p.Name).Take(3);
        var lowStock = _inventoryStore.GetAll().Where(p => p.Stock <= 10).ToList();
        var forecast = _salesForecastService.BuildForecast();

        return "Resumen del minimarket:\n" +
               $"- Ventas proyectadas promedio diarios: S/ {forecast.AverageDailySales:0.00}.\n" +
               (lowStock.Any() ? $"- Productos con stock bajo: {string.Join(", ", lowStock.Select(p => p.Name))}.\n" : "- Inventario saludable en general.\n") +
               "- Productos destacados: " + string.Join(", ", topProducts.Select(p => p.Name)) + ".\n" +
               "Puedes preguntar sobre inventario, ventas, recomendaciones o promociones.";
    }
}
