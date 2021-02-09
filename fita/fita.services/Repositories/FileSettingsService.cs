using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class FileSettingsService : RepositoryService<FileSettings>
    {
        public FileSettingsService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }
        
        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.FileSettingsId);
        }
    }
}