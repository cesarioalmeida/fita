using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class FileSettingsRepoService : RepositoryService<FileSettings>, ISeedData
    {
        public FileSettingsRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }
        
        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.FileSettingsId);
        }

        public Task<Result> SeedData()
        {
            return Task.Run(async () =>
            {
                var file = new FileSettings
                {
                    Name = "New File"
                };

                if (Collection.FindOne(x => x.Name == file.Name) == null)
                {
                    return await SaveAsync(file);
                }

                return Result.Success;
            });
        }
    }
}