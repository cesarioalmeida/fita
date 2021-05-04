using System;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Core
{
    public class ExchangeRateService : IExchangeRateService
    {
        private static IConfiguration configuration =
            new ConfigurationBuilder().AddUserSecrets<ExchangeRateService>().Build();

        public ExchangeRateRepoService ExchangeRateRepoService { get; set; }

        public HistoricalDataRepoService HistoricalDataRepoService { get; set; }

        public ILoggingService LoggingService { get; set; }

        public Task<Result> UpdateAsync(ExchangeRate exchangeRate, DateTime? date = null)
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
                        var data = DownloadData(exchangeRate, date);

                        if (exchangeRate.Rate == null)
                        {
                            PrepareHistoricalData(exchangeRate);
                        }

                        if (exchangeRate.Rate?.DataPoints.SingleOrDefault(x => x.Date.Date == data.Date.Date) is var
                            existingDataPoint and { })
                        {
                            existingDataPoint.Value = data.Value;
                        }
                        else
                        {
                            exchangeRate.Rate?.DataPoints.Add(new HistoricalDataPoint
                                {Date = data.Date.Date, Value = data.Value});
                        }

                        if (await HistoricalDataRepoService.SaveAsync(exchangeRate.Rate))
                        {
                            return await ExchangeRateRepoService.SaveAsync(exchangeRate);
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

        public async Task<decimal> Exchange(Currency fromCurrency, Currency toCurrency, decimal value,
            DateTime? date = null)
        {
            try
            {
                var exchangeRate = await ExchangeRateRepoService.FromToCurrencyEnrichedAsync(fromCurrency, toCurrency);

                if (date == null)
                {
                    return (exchangeRate == null ? 1m : exchangeRate.Rate.LatestValue ?? 1m) * value;
                }

                if (exchangeRate == null)
                {
                    return 1m * value;
                }

                if (!exchangeRate.Rate.DataPoints.Select(x => x.Date.Date).Contains(date.Value.Date))
                {
                    await UpdateAsync(exchangeRate, date);
                }

                return (exchangeRate.Rate.DataPoints.SingleOrDefault(x => x.Date.Date == date.Value.Date)?.Value ??
                        1m) * value;
            }
            catch (Exception ex)
            {
                LoggingService.Warn($"{nameof(Convert)}: {ex}");
                return 1m * value;
            }
        }

        private static HistoricalElement DownloadData(ExchangeRate exchangeRate, DateTime? date = null)
        {
            var requestUrl = date == null
                ? $"{Properties.Resources.ExchangeRateApi}?access_key={configuration["ExchangeRatesApi"]}&base={exchangeRate.FromCurrency.Symbol}&symbols={exchangeRate.ToCurrency.Symbol}"
                : $"{Properties.Resources.ExchangeRateApi}?access_key={configuration["ExchangeRatesApi"]}&start_at={date.Value:YYYY-MM-dd}&end_at={date.Value:YYYY-MM-dd}&base={exchangeRate.FromCurrency.Symbol}&symbols={exchangeRate.ToCurrency.Symbol}";

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
                Name = $"Exchange rate {rate.FromCurrency.Name} => {rate.ToCurrency.Name}"
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