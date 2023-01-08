using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class SecurityPositionRepoService : RepositoryService<SecurityPosition>
{
    public SecurityPositionRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.SecurityPositionId);
        Collection.EnsureIndex(x => x.Security);
        Collection.EnsureIndex(x => x.AccountId);
    }

    public Task<IEnumerable<SecurityPosition>> AllEnrichedForAccount(ObjectId accountId)
        => Task.Run(
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
                    LoggingService.Warn($"{nameof(AllEnrichedForAccount)}: {ex}");
                    return null;
                }
            });

    public Task<SecurityPosition> DetailsForSecurityEnriched(ObjectId securityId)
        => Task.Run(
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
                    LoggingService.Warn($"{nameof(DetailsForSecurityEnriched)}: {ex}");
                    return null;
                }
            });
}