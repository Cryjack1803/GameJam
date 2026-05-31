using Microsoft.ML;
using Microsoft.ML.Data;
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

        var mlContext = new MLContext(seed: 1);
        var trainingData = mlContext.Data.LoadFromEnumerable(dailySales);

        var pipeline = mlContext.Transforms.Concatenate("Features", nameof(SalesObservation.DayIndex))
            .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: nameof(SalesObservation.Total), maximumNumberOfIterations: 100));

        var model = pipeline.Fit(trainingData);
        var engine = mlContext.Model.CreatePredictionEngine<SalesObservation, SalesPrediction>(model);

        var lastDate = orderedSales.Last().CreatedAt.Date;
        var forecastPoints = new List<SalesForecastPointViewModel>();

        for (var index = 1; index <= horizonDays; index++)
        {
            var prediction = engine.Predict(new SalesObservation { DayIndex = dailySales.Count + index });
            var predictedTotal = Math.Max(0, (decimal)prediction.Score);
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
            AverageDailySales = decimal.Round(orderedSales.Average(sale => sale.Total), 2),
            NextDayPrediction = forecastPoints.FirstOrDefault()?.PredictedTotal ?? 0,
            ForecastPoints = forecastPoints
        };
    }

    private sealed class SalesObservation
    {
        public float DayIndex { get; set; }

        public float Total { get; set; }
    }

    private sealed class SalesPrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}