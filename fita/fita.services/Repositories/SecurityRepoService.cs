using System.ComponentModel.Composition;
using fita.data.Models;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class SecurityRepoService : RepositoryService<Security>
{
    public SecurityRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)  
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.SecurityId);
        Collection.EnsureIndex(x => x.Name);
        Collection.EnsureIndex(x => x.Symbol);
        Collection.EnsureIndex(x => x.Type);
    }
}