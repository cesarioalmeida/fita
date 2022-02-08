using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class AccountRepoService : RepositoryService<Account>
    {
        public AccountRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.AccountId);
            Collection.EnsureIndex(x => x.Name);
            Collection.EnsureIndex(x => x.Type);
        }
    }
}