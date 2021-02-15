using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;
using LiteDB;
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

        public Task<Result> UpdateAllAsync()
        {
            return null;
        }

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
                        var requestUrl = $"{Properties.Resources.ExchangeRateApi}?base={exchangeRate.FromCurrency.Symbol}&symbols={exchangeRate.ToCurrency.Symbol}";

                        using var client = new WebClientExtended {Timeout = 5000};

                        var json = client.DownloadString(requestUrl);
                        dynamic parsedJson = JObject.Parse(json);

                        var data = new
                        {
                            Date = (DateTime)Convert.ToDateTime(parsedJson.date),
                            Value = (decimal)Convert.ToDecimal(parsedJson.rates[$"{exchangeRate.ToCurrency.Symbol}"])
                        };

                        if (data.Date.Date == exchangeRate.Rate.LatestDate?.Date)
                        {
                            exchangeRate.Rate.Data[data.Date.Date].Value = data.Value;
                        }
                        else
                        {
                            var historicalData = new HistoricalData
                            {
                                HistoricalDataId = ObjectId.NewObjectId(),
                                Name = $"Exchange rate {exchangeRate.FromCurrency.Name} => {exchangeRate.ToCurrency.Name}",
                                Data = new SortedDictionary<DateTime, HistoricalPoint>
                                {
                                    {data.Date, new HistoricalPoint { Date = data.Date, Value = data.Value}}
                                }
                            };

                            exchangeRate.Rate = historicalData;
                        }

                        return await SaveDataAsync(exchangeRate);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(UpdateAsync)}: {ex}");
                        return Result.Fail;
                    }
                });
        }

        private async Task<Result> SaveDataAsync(ExchangeRate exchange)
        {
            if (await HistoricalDataService.SaveAsync(exchange.Rate))
            {
                return await ExchangeRateService.SaveAsync(exchange);
            }

            return Result.Fail;
        }
    }
}