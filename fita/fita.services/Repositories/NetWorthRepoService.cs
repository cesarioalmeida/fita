using System;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class NetWorthRepoService : RepositoryService<NetWorth>
    {
        public NetWorthRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(
            dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.NetWorthId);
            Collection.EnsureIndex(x => x.Date);
        }

        public Task<NetWorth> GetForDateAsync(DateTime date)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.FindOne(x => x.Date.Date == date.Date);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(GetForDateAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}