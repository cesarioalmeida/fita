using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class SecurityHistoryRepoService : RepositoryService<SecurityHistory>
{
    public SecurityHistoryRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService) 
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.SecurityHistoryId);
        Collection.EnsureIndex(x => x.Security);
    }

    public Task<SecurityHistory> FromSecurityEnriched(Security security)
    {
        if (security is null)
        {
            return null;
        }

        return Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.Security)
                        .Include(x => x.Price)
                        .FindOne(x => x.Security.SecurityId == security.SecurityId);
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(FromSecurityEnriched)}: {ex}");
                    return null;
                }
            });
    }
}