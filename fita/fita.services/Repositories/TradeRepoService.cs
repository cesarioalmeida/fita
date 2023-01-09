using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class TradeRepoService : RepositoryService<Trade>
{
    public TradeRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.TradeId);
        Collection.EnsureIndex(x => x.AccountId);
        Collection.EnsureIndex(x => x.Date);
        Collection.EnsureIndex(x => x.Action);
    }

    public override async Task<IEnumerable<Trade>> GetAll(bool enriched = false)
        => (await base.GetAll(enriched))
            .OrderBy(x => x.Date);

    public async Task<IEnumerable<Trade>> GetAllToDateForAccount(DateTime endDate, ObjectId accountId)
    {
        try
        {
            return (await GetAll(true)).Where(x => x.AccountId == accountId && x.Date <= endDate).OrderBy(x => x.Date);
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllToDateForAccount)}: {ex}");
            return null;
        }
    }
}