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

        public Task<Result> Update(SecurityHistory securityHistory, DateTime? date = null)
        {
            return Task.Run(
                async () =>
                {
                    if (securityHistory is null || string.IsNullOrEmpty(securityHistory.Security.Symbol))
                    {
                        return Result.Fail;
                    }

                    try
                    {
                        var data = await DownloadData(securityHistory, date);

                        if (securityHistory.Price is null)
                        {
                            PrepareHistoricalData(securityHistory);
                        }

                        if (securityHistory.Price?.DataPoints.SingleOrDefault(x => x.Date.Date == data.Date.Date) is { } existingDataPoint)
                        {
                            existingDataPoint.Value = data.Value;
                        }
                        else
                        {
                            securityHistory.Price?.DataPoints.Add(new() {Date = data.Date.Date, Value = data.Value});
                        }

                        return await HistoricalDataRepoService.SaveAsync(securityHistory.Price)
                            ? await SecurityHistoryRepoService.SaveAsync(securityHistory)
                            : Result.Success;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(Update)}: {ex}");
                        return Result.Fail;
                    }
                });
        }

        private static async Task<HistoricalElement> DownloadData(SecurityHistory securityHistory, DateTime? date = null)
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

            const int timeout = 5000;

            while(yahooHistorical.Count == 0 && numberOfTries < 3)
            {
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

        private record HistoricalElement(DateTime Date, decimal Value);
    }
}