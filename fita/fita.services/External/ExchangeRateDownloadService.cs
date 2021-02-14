using System;
using System.Linq;
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

                        using var client = new WebClientExtended();

                        var json = client.DownloadString(requestUrl);
                        dynamic parsedJson = JObject.Parse(json);

                        var data = new
                        {
                            Date = (DateTime)Convert.ToDateTime(parsedJson.date),
                            Value = (decimal)Convert.ToDecimal(parsedJson.rates[$"{exchangeRate.ToCurrency.Symbol}"])
                        };

                        var latestHistoricalDataForDate =
                            exchangeRate.HistoricalData.FirstOrDefault(x => x.Date.Date == data.Date.Date);

                        if (latestHistoricalDataForDate != null)
                        {
                            latestHistoricalDataForDate.Date = data.Date;
                            latestHistoricalDataForDate.Value = data.Value;
                        }
                        else
                        {
                            latestHistoricalDataForDate = new HistoricalData
                            {
                                HistoricalDataId = ObjectId.NewObjectId(),
                                Date = data.Date,
                                Value = data.Value
                            };

                            exchangeRate.HistoricalData.Add(latestHistoricalDataForDate);
                        }

                        if (await HistoricalDataService.SaveAsync(latestHistoricalDataForDate))
                        {
                            return await ExchangeRateService.SaveAsync(exchangeRate);
                        }

                        return Result.Fail;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(UpdateAsync)}: {ex}");
                        return Result.Fail;
                    }
                });
        }
    }
}