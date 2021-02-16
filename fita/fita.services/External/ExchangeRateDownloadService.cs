using System;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;
using Newtonsoft.Json.Linq;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.External
{
    public class ExchangeRateDownloadService : IExchangeRateDownloadService
    {
        public ExchangeRateService ExchangeRateService { get; set; }

        public HistoricalDataService HistoricalDataService { get; set; }

        public ILoggingService LoggingService { get; set; }

        public Task<Result> UpdateAsync(ExchangeRate exchangeRate)
        {
            return Task.Run(
                async () =>
                {
                    if (exchangeRate == null || string.IsNullOrEmpty(exchangeRate.FromCurrency.Symbol) ||
                        string.IsNullOrEmpty(exchangeRate.ToCurrency.Symbol))
                    {
                        return Result.Fail;
                    }

                    try
                    {
                        var data = DownloadData(exchangeRate);

                        if (exchangeRate.Rate == null)
                        {
                            PrepareHistoricalData(exchangeRate);
                        }

                        if (data.Date.Date == exchangeRate.Rate?.LatestDate?.Date)
                        {
                            exchangeRate.Rate.Data[data.Date.Date].Value = data.Value;
                        }
                        else
                        {
                            exchangeRate.Rate?.Data.Add(data.Date,
                                new HistoricalPoint {Date = data.Date, Value = data.Value});
                        }

                        if(await HistoricalDataService.SaveAsync(exchangeRate.Rate))
                        {
                            return await ExchangeRateService.SaveAsync(exchangeRate);
                        }

                        return Result.Success;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(UpdateAsync)}: {ex}");
                        return Result.Fail;
                    }
                });
        }

        private static HistoricalElement DownloadData(ExchangeRate exchangeRate)
        {
            var requestUrl =
                $"{Properties.Resources.ExchangeRateApi}?base={exchangeRate.FromCurrency.Symbol}&symbols={exchangeRate.ToCurrency.Symbol}";

            using var client = new WebClientExtended {Timeout = 5000};

            var json = client.DownloadString(requestUrl);
            dynamic parsedJson = JObject.Parse(json);

            return new HistoricalElement((DateTime) Convert.ToDateTime(parsedJson.date),
                (decimal) Convert.ToDecimal(parsedJson.rates[$"{exchangeRate.ToCurrency.Symbol}"]));
        }

        private static void PrepareHistoricalData(ExchangeRate rate)
        {
            var historicalData = new HistoricalData
            {
                Name = $"Exchange rate {rate.FromCurrency.Name} => {rate.ToCurrency.Name}",
            };

            rate.Rate = historicalData;
        }

        internal readonly struct HistoricalElement
        {
            public HistoricalElement(DateTime date, decimal value)
            {
                Date = date;
                Value = value;
            }

            public DateTime Date { get; }

            public decimal Value { get; }
        }
    }
}