using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;
using twentySix.Framework.Core.Services.Interfaces;
using YahooFinanceAPI;
using YahooFinanceAPI.Models;

namespace fita.services.Core
{
    public class SecurityService : ISecurityService
    {
        public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }

        public HistoricalDataRepoService HistoricalDataRepoService { get; set; }

        public ILoggingService LoggingService { get; set; }

        public Task<Result> UpdateAsync(SecurityHistory securityHistory, DateTime? date = null)
        {
            return Task.Run(
                async () =>
                {
                    if (securityHistory == null || string.IsNullOrEmpty(securityHistory.Security.Symbol))
                    {
                        return Result.Fail;
                    }

                    try
                    {
                        var data = await DownloadDataAsync(securityHistory, date);

                        if (securityHistory.Price == null)
                        {
                            PrepareHistoricalData(securityHistory);
                        }

                        if (securityHistory.Price?.DataPoints.SingleOrDefault(x => x.Date.Date == data.Date.Date) is { } existingDataPoint)
                        {
                            existingDataPoint.Value = data.Value;
                        }
                        else
                        {
                            securityHistory.Price?.DataPoints.Add(new HistoricalDataPoint
                                {Date = data.Date.Date, Value = data.Value});
                        }

                        if (await HistoricalDataRepoService.SaveAsync(securityHistory.Price))
                        {
                            return await SecurityHistoryRepoService.SaveAsync(securityHistory);
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

        private static async Task<HistoricalElement> DownloadDataAsync(SecurityHistory securityHistory, DateTime? date = null)
        {
            List<YahooHistoryPrice> yahooHistorical = new();

            var numberOfTries = 0;
            var selectedDate = date ?? DateTime.Now;

            if (selectedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                selectedDate = selectedDate.DayOfWeek == DayOfWeek.Saturday
                    ? selectedDate.AddDays(-1)
                    : selectedDate.AddDays(-2);
            }

            var timeout = 5000;

            while(yahooHistorical.Count == 0 && numberOfTries < 3)
            {
                //yahooHistorical = await YahooHistorical.GetPriceAsync(securityHistory.Security.Symbol, selectedDate.Date, selectedDate);
                var downloadTask = YahooHistorical.GetPriceAsync(securityHistory.Security.Symbol, selectedDate.Date, selectedDate);
                if (await Task.WhenAny(downloadTask, Task.Delay(timeout)) == downloadTask)
                {
                    yahooHistorical = downloadTask.Result;
                }

                numberOfTries++;
            }

            return new HistoricalElement(yahooHistorical.First().Date, (decimal)yahooHistorical.First().Close);
        }

        private static void PrepareHistoricalData(SecurityHistory securityHistory)
        {
            var historicalData = new HistoricalData
            {
                Name = $"Price History for {securityHistory.Security.Name}"
            };

            securityHistory.Price = historicalData;
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