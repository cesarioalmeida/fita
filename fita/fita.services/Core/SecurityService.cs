using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Services.Interfaces;
using YahooFinanceAPI;
using YahooFinanceAPI.Models;

namespace fita.services.Core;

[Export(typeof(ISecurityService))]
public class SecurityService : ISecurityService
{
    [Import] public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }

    [Import] public HistoricalDataRepoService HistoricalDataRepoService { get; set; }

    [Import] public ILoggingService LoggingService { get; set; }

    public Task<Result> Update(SecurityHistory securityHistory, DateTime? date = null)
        => Task.Run(
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

                    if (securityHistory.Price?.DataPoints.SingleOrDefault(x => x.Date.Date == data.Date.Date) is
                        { } existingDataPoint)
                    {
                        existingDataPoint.Value = data.Value;
                    }
                    else
                    {
                        securityHistory.Price?.DataPoints.Add(new() {Date = data.Date.Date, Value = data.Value});
                    }

                    return await HistoricalDataRepoService.Save(securityHistory.Price)
                        ? await SecurityHistoryRepoService.Save(securityHistory)
                        : Result.Success;
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(Update)}: {ex}");
                    return Result.Fail;
                }
            });

    private static async Task<HistoricalElement> DownloadData(SecurityHistory securityHistory, DateTime? date = null)
    {
        List<YahooHistoryPrice> yahooHistorical = new();

        var numberOfTries = 0;
        var startDate = date ?? DateTime.Now;

        if (startDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            startDate = startDate.DayOfWeek == DayOfWeek.Saturday
                ? startDate.AddDays(-1)
                : startDate.AddDays(-2);
        }

        const int timeout = 5000;

        while (yahooHistorical.Count == 0 && numberOfTries < 3)
        {
            var downloadTask = YahooHistorical.GetPriceAsync(securityHistory.Security.Symbol, startDate.Date, DateTime.Now);
            if (await Task.WhenAny(downloadTask, Task.Delay(timeout)) == downloadTask)
            {
                yahooHistorical = downloadTask.Result;
            }

            numberOfTries++;
        }

        return new HistoricalElement(yahooHistorical.Last().Date, (decimal) yahooHistorical.Last().Close);
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