using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.services.Repositories;

[Export]
public class FileSettingsRepoService : RepositoryService<FileSettings>, ISeedData
{
    public FileSettingsRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)
        => IndexData();

    public sealed override void IndexData()
        => Collection.EnsureIndex(x => x.FileSettingsId);

    public Task<Result> SeedData()
        => Task.Run(async () =>
        {
            var file = new FileSettings
            {
                Name = "New File"
            };

            if (Collection.FindOne(x => x.Name == file.Name) is null)
            {
                return await Save(file);
            }

            return Result.Success;
        });
}