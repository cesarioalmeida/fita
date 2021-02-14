using System;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class HistoricalDataService : RepositoryService<HistoricalData>
    {
        public HistoricalDataService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(
            dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.HistoricalDataId);
            Collection.EnsureIndex(x => x.Date);
        }

        public Task<HistoricalData> LatestForDateAsync(DateTime date)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.Find(x => x.Date.Date == date.Date).OrderByDescending(x => x.Date)
                            .FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(LatestForDateAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}