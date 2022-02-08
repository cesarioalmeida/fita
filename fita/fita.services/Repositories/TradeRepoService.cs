using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class TradeRepoService : RepositoryService<Trade>
    {
        public TradeRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.TradeId);
            Collection.EnsureIndex(x => x.AccountId);
            Collection.EnsureIndex(x => x.Date);
            Collection.EnsureIndex(x => x.Action);
        }

        public override async Task<IEnumerable<Trade>> AllAsync()
        {
            return (await base.AllAsync())
                .OrderBy(x => x.Date)
                .AsEnumerable();
        }

        public override Task<IEnumerable<Trade>> AllEnrichedAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
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

        public Task<IEnumerable<Trade>> AllEnrichedForAccountAsync(ObjectId accountId)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
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
    }
}