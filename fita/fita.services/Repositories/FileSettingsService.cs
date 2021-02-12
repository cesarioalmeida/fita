using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class FileSettingsService : RepositoryService<FileSettings>, ISeedData
    {
        public FileSettingsService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
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
                    FileSettingsId= ObjectId.NewObjectId(),
                    Name = "New File"
                };

                if (Collection.FindOne(x => x.Name == file.Name) == null)
                {
                    return await SaveAsync(file);
                }

                return Result.Success;
            });
        }

        public override Task<FileSettings> DetailsEnrichedAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.BaseCurrency)
                            .FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public override Task<IEnumerable<FileSettings>> AllEnrichedAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.BaseCurrency)
                            .FindAll();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}