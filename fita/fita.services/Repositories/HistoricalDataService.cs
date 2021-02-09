using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class HistoricalDataService : RepositoryService<HistoricalData>
    {
        public HistoricalDataService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }
        
        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.HistoricalDataId);
            Collection.EnsureIndex(x => x.Date);
        }
    }
}