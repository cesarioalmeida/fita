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
public class TransactionRepoService : RepositoryService<Transaction>
{
    public TransactionRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.TransactionId);
        Collection.EnsureIndex(x => x.AccountId);
        Collection.EnsureIndex(x => x.Date);
        Collection.EnsureIndex(x => x.Category);
    }

    public override async Task<IEnumerable<Transaction>> GetAll(bool enriched = false)
        => (await base.GetAll(enriched))
            .OrderBy(x => x.Date);

    public async Task<IEnumerable<Transaction>> GetAllForAccount(ObjectId accountId)
    {
        try
        {
            return (await GetAllConditional(x => x.AccountId == accountId, true)).OrderBy(x => x.Date);
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllForAccount)}: {ex}");
            return null;
        }
    }

    public async Task<IEnumerable<Transaction>> GetAllBetweenDates(DateTime startDate, DateTime? endDate = null)
    {
        try
        {
            endDate ??= DateTime.MaxValue;

            return (await GetAllConditional(x => x.Date >= startDate && x.Date <= endDate, true)).OrderBy(x => x.Date);
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllBetweenDates)}: {ex}");
            return null;
        }
    }

    public async Task<IEnumerable<Transaction>> GetAllToDateForAccount(DateTime endDate, ObjectId accountId)
    {
        try
        {
            return (await GetAllConditional(x => x.Date <= endDate && x.AccountId == accountId, true)).OrderBy(x => x.Date);
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllToDateForAccount)}: {ex}");
            return null;
        }
    }
}