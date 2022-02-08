using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class SecurityPositionRepoService : RepositoryService<SecurityPosition>
    {
        public SecurityPositionRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.SecurityPositionId);
            Collection.EnsureIndex(x => x.Security);
            Collection.EnsureIndex(x => x.AccountId);
        }

        public Task<IEnumerable<SecurityPosition>> AllEnrichedForAccountAsync(ObjectId accountId)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .Find(x => x.AccountId == accountId && x.Quantity > 0);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedForAccountAsync)}: {ex}");
                        return null;
                    }
                });
        }
        
        public Task<SecurityPosition> DetailsForSecurityEnrichedAsync(ObjectId securityId)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .FindOne(x => x.Security.SecurityId == securityId);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsForSecurityEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}