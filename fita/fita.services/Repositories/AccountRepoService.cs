using System.ComponentModel.Composition;
using fita.data.Models;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class AccountRepoService : RepositoryService<Account>
{
    public AccountRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService) 
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.AccountId);
        Collection.EnsureIndex(x => x.Name);
        Collection.EnsureIndex(x => x.Type);
    }
}