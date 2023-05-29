using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Core;

[Export(typeof(IExchangeRateService))]
public class ExchangeRateService : IExchangeRateService
{
    private static readonly IConfiguration Configuration =
        new ConfigurationBuilder().AddUserSecrets<ExchangeRateService>().Build();

    private readonly ConcurrentDictionary<string, ExchangeRate> _exchangeRatesCache = new();

    [Import] public ExchangeRateRepoService ExchangeRateRepoService { get; set; }

    [Import] public HistoricalDataRepoService HistoricalDataRepoService { get; set; }

    [Import] public ILoggingService LoggingService { get; set; }

    public Task<Result> Update(ExchangeRate exchangeRate, DateTime? date = null)
    {
        return Task.Run(
            async () =>
            {
                if (exchangeRate is null || string.IsNullOrEmpty(exchangeRate.FromCurrency.Symbol) ||
                    string.IsNullOrEmpty(exchangeRate.ToCurrency.Symbol))
                {
                    return Result.Fail;
                }
                
                // update cache
                _exchangeRatesCache[$"{exchangeRate.FromCurrency.Symbol}-{exchangeRate.ToCurrency.Symbol}"] = exchangeRate;

                try
                {
                    var (day, rate) = await DownloadData(exchangeRate, date);

                    if (exchangeRate.Rate is null)
                    {
                        PrepareHistoricalData(exchangeRate);
                    }

                    if (exchangeRate.Rate?.DataPoints.SingleOrDefault(x => x.Date.Date == day.Date) is
                        { } existingDataPoint)
                    {
                        existingDataPoint.Value = rate;
                    }
                    else
                    {
                        exchangeRate.Rate?.DataPoints.Add(new HistoricalDataPoint {Date = day.Date, Value = rate});
                    }

                    return await HistoricalDataRepoService.Save(exchangeRate.Rate)
                        ? await ExchangeRateRepoService.Save(exchangeRate)
                        : Result.Success;
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(Update)}: {ex}");
                    return Result.Fail;
                }
            });
    }

    public async Task<decimal> Exchange(Currency fromCurrency, Currency toCurrency, decimal value, DateTime? date = null)
    {
        try
        {
            var key = $"{fromCurrency.Symbol}-{toCurrency.Symbol}";

            if (!_exchangeRatesCache.TryGetValue(key, out var exchangeRate))
            {
                exchangeRate = await ExchangeRateRepoService.GetSingleFromToCurrency(fromCurrency, toCurrency);
                _exchangeRatesCache[key] = exchangeRate;
            }
            
            if (date is null)
            {
                return (exchangeRate is null ? 1m : exchangeRate.Rate.LatestValue ?? 1m) * value;
            }

            if (exchangeRate is null)
            {
                return 1m * value;
            }

            if (!exchangeRate.Rate.DataPoints.Select(x => x.Date.Date).Contains(date.Value.Date))
            {
                await Update(exchangeRate, date);
            }

            return (exchangeRate.Rate.DataPoints.SingleOrDefault(x => x.Date.Date == date.Value.Date)?.Value ?? 1m) *
                   value;
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(Convert)}: {ex}");
            return 1m * value;
        }
    }

    private static async Task<HistoricalElement> DownloadData(ExchangeRate exchangeRate, DateTime? date = null)
    {
        var requestUrl = date is null
            ? $"{Properties.Resources.ExchangeRateApi}?base={exchangeRate.FromCurrency.Symbol}&symbols={exchangeRate.ToCurrency.Symbol}"
            : $"{Properties.Resources.ExchangeRateApi}?start_at={date.Value:YYYY-MM-dd}&end_at={date.Value:YYYY-MM-dd}&base={exchangeRate.FromCurrency.Symbol}&symbols={exchangeRate.ToCurrency.Symbol}";

        using var client = new HttpClient {Timeout = TimeSpan.FromSeconds(5)};

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(requestUrl),
            Headers =
            {
                {"X-RapidAPI-Host", "fixer-fixer-currency-v1.p.rapidapi.com"},
                {"X-RapidAPI-Key", $"{Configuration["ExchangeRatesApi"]}"},
            },
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        dynamic parsedJson = JObject.Parse(json);

        return new HistoricalElement((DateTime) Convert.ToDateTime(parsedJson.date),
            (decimal) Convert.ToDecimal(parsedJson.rates[$"{exchangeRate.ToCurrency.Symbol}"]));
    }

    private static void PrepareHistoricalData(ExchangeRate rate)
    {
        var historicalData = new HistoricalData
            {Name = $"Exchange rate {rate.FromCurrency.Name} => {rate.ToCurrency.Name}"};
        rate.Rate = historicalData;
    }

    private record HistoricalElement(DateTime Date, decimal Value);
}