using System;
using System.Collections.Generic;
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

        public override Task<Transaction> DetailsEnrichedAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Category)
                            .Include(x => x.TransferAccount)
                            .FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
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
                            .Include(x => x.TransferAccount)
                            .FindAll();
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
                            .Include(x => x.TransferAccount)
                            .Find(x => x.AccountId == accountId);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedForAccountAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}