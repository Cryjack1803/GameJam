namespace Tarea_01.Models;

public class SalesForecastPointViewModel
{
    public DateTime Date { get; set; }

    public decimal PredictedTotal { get; set; }

    public decimal LowerBound { get; set; }

    public decimal UpperBound { get; set; }
}