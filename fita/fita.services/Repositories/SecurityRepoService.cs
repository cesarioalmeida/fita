using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class SecurityRepoService : RepositoryService<Security>
    {
        public SecurityRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.SecurityId);
            Collection.EnsureIndex(x => x.Name);
            Collection.EnsureIndex(x => x.Symbol);
            Collection.EnsureIndex(x => x.Type);
        }
    }
}