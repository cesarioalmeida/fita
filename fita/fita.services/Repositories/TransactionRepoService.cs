using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class TransactionRepoService : RepositoryService<Transaction>
    {
        public TransactionRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.TransactionId);
            Collection.EnsureIndex(x => x.AccountId);
            Collection.EnsureIndex(x => x.Date);
            Collection.EnsureIndex(x => x.Category);
        }

        public override async Task<IEnumerable<Transaction>> AllAsync()
        {
            return (await base.AllAsync())
                .OrderBy(x => x.Date)
                .AsEnumerable();
        }

        public override Task<IEnumerable<Transaction>> AllEnrichedAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Category)
                            .FindAll()
                            .OrderBy(x => x.Date)
                            .AsEnumerable();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<IEnumerable<Transaction>> AllEnrichedForAccountAsync(ObjectId accountId)
        {
            return Task.Run(
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
                        LoggingService.Warn($"{nameof(AllEnrichedForAccountAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<IEnumerable<Transaction>> AllEnrichedBetweenDatesAsync(DateTime startDate, DateTime? endDate = null)
        {
            return Task.Run(
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
                        LoggingService.Warn($"{nameof(AllEnrichedBetweenDatesAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}