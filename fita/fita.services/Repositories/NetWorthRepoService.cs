using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class NetWorthRepoService : RepositoryService<NetWorth>
{
    public NetWorthRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService) 
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.NetWorthId);
        Collection.EnsureIndex(x => x.Date);
    }

    public Task<NetWorth> GetForDate(DateTime date)
        => Task.Run(
            () =>
            {
                try
                {
                    return Collection.FindOne(x => x.Date.Date == date.Date);
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(GetForDate)}: {ex}");
                    return null;
                }
            });
}