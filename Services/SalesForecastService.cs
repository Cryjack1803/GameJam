using Tarea_01.Models;

namespace Tarea_01.Services;

public class SalesForecastService
{
    private readonly SalesHistoryStore _salesHistoryStore;

    public SalesForecastService(SalesHistoryStore salesHistoryStore)
    {
        _salesHistoryStore = salesHistoryStore;
    }

    public SalesForecastViewModel BuildForecast(int horizonDays = 7)
    {
        var orderedSales = _salesHistoryStore.GetAll()
            .OrderBy(sale => sale.CreatedAt)
            .ToList();

        var dailySales = orderedSales
            .GroupBy(sale => sale.CreatedAt.Date)
            .Select((group, index) => new SalesObservation
            {
                DayIndex = index + 1,
                Total = (float)group.Sum(item => item.Total)
            })
            .ToList();

        if (dailySales.Count == 0)
        {
            return new SalesForecastViewModel();
        }

        var lastDate = orderedSales.Last().CreatedAt.Date;
        var forecastPoints = new List<SalesForecastPointViewModel>();
        var trendProjection = BuildTrendProjection(dailySales, horizonDays);

        for (var index = 1; index <= horizonDays; index++)
        {
            var predictedTotal = trendProjection[index - 1];
            var lower = Math.Max(0, predictedTotal * 0.9m);
            var upper = predictedTotal * 1.1m;

            forecastPoints.Add(new SalesForecastPointViewModel
            {
                Date = lastDate.AddDays(index),
                PredictedTotal = decimal.Round(predictedTotal, 2),
                LowerBound = decimal.Round(lower, 2),
                UpperBound = decimal.Round(upper, 2)
            });
        }

        return new SalesForecastViewModel
        {
            LastObservedTotal = decimal.Round((decimal)dailySales.Last().Total, 2),
            AverageDailySales = decimal.Round(dailySales.Average(sale => (decimal)sale.Total), 2),
            NextDayPrediction = forecastPoints.FirstOrDefault()?.PredictedTotal ?? 0,
            ForecastPoints = forecastPoints
        };
    }

    private static List<decimal> BuildTrendProjection(IReadOnlyList<SalesObservation> dailySales, int horizonDays)
    {
        if (dailySales.Count == 1)
        {
            var singleValue = decimal.Round((decimal)dailySales[0].Total, 2);
            return Enumerable.Range(0, horizonDays).Select(_ => singleValue).ToList();
        }

        var pointCount = dailySales.Count;
        decimal sumX = 0;
        decimal sumY = 0;
        decimal sumXY = 0;
        decimal sumX2 = 0;

        foreach (var point in dailySales)
        {
            var x = (decimal)point.DayIndex;
            var y = (decimal)point.Total;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        var n = (decimal)pointCount;
        var denominator = (n * sumX2) - (sumX * sumX);
        var slope = denominator == 0 ? 0 : ((n * sumXY) - (sumX * sumY)) / denominator;
        var intercept = n == 0 ? 0 : (sumY - (slope * sumX)) / n;

        var recentTotals = dailySales.TakeLast(Math.Min(5, pointCount)).Select(item => (decimal)item.Total).ToList();
        var recentAverage = recentTotals.Average();
        var momentum = recentTotals.Count > 1
            ? (recentTotals.Last() - recentTotals.First()) / (recentTotals.Count - 1)
            : 0;

        var forecasts = new List<decimal>(horizonDays);

        for (var index = 1; index <= horizonDays; index++)
        {
            var futureX = pointCount + index;
            var regressionValue = intercept + (slope * futureX);
            var momentumValue = recentTotals.Last() + (momentum * index);
            var blendedValue = (regressionValue * 0.55m) + (recentAverage * 0.30m) + (momentumValue * 0.15m);
            forecasts.Add(decimal.Round(Math.Max(0, blendedValue), 2));
        }

        return forecasts;
    }

    private sealed class SalesObservation
    {
        public float DayIndex { get; set; }

        public float Total { get; set; }
    }
}