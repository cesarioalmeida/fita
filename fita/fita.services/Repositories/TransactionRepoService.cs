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
            .OrderBy(x => x.Date)
            .AsEnumerable();

    public Task<IEnumerable<Transaction>> AllEnrichedForAccount(ObjectId accountId)
        => Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.Category)
                        .Find(x => x.AccountId == accountId)
                        .OrderBy(x => x.Date)
                        .AsEnumerable();
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(AllEnrichedForAccount)}: {ex}");
                    return null;
                }
            });

    public Task<IEnumerable<Transaction>> AllEnrichedBetweenDates(DateTime startDate, DateTime? endDate = null)
        => Task.Run(
            () =>
            {
                try
                {
                    endDate ??= DateTime.MaxValue;

                    return Collection
                        .Include(x => x.Category)
                        .Find(x => x.Date >= startDate && x.Date <= endDate)
                        .OrderBy(x => x.Date)
                        .AsEnumerable();
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(AllEnrichedBetweenDates)}: {ex}");
                    return null;
                }
            });

    public Task<IEnumerable<Transaction>> AllEnrichedToDate(DateTime endDate)
        => Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.Category)
                        .Find(x => x.Date <= endDate)
                        .OrderBy(x => x.Date)
                        .AsEnumerable();
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(AllEnrichedToDate)}: {ex}");
                    return null;
                }
            });

    public Task<IEnumerable<Transaction>> AllEnrichedToDateForAccount(DateTime endDate, ObjectId accountId)
        => Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.Category)
                        .Find(x => x.Date <= endDate && x.AccountId == accountId)
                        .OrderBy(x => x.Date)
                        .AsEnumerable();
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(AllEnrichedToDateForAccount)}: {ex}");
                    return null;
                }
            });
}