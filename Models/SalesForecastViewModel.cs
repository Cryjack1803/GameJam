namespace Tarea_01.Models;

public class SalesForecastViewModel
{
    public decimal LastObservedTotal { get; set; }

    public decimal AverageDailySales { get; set; }

    public decimal NextDayPrediction { get; set; }

    public List<SalesForecastPointViewModel> ForecastPoints { get; set; } = new();
}