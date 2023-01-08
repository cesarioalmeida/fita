using System.ComponentModel.Composition;
using fita.data.Models;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    [Export]
    public class HistoricalDataRepoService : RepositoryService<HistoricalData>
    {
        public HistoricalDataRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
            [Import] ILoggingService loggingService)
            : base(dbHelperServiceFactory.GetInstance(), loggingService) 
            => IndexData();

        public sealed override void IndexData() 
            => Collection.EnsureIndex(x => x.HistoricalDataId);
    }
}