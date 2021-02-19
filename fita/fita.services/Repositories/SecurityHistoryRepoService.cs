using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class SecurityHistoryRepoService : RepositoryService<SecurityHistory>
    {
        public SecurityHistoryRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(
            dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.SecurityHistoryId);
            Collection.EnsureIndex(x => x.Security);
        }

        public override Task<SecurityHistory> DetailsEnrichedAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .Include(x => x.Price)
                            .FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public override Task<IEnumerable<SecurityHistory>> AllEnrichedAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .Include(x => x.Price)
                            .FindAll();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<SecurityHistory> FromSecurityEnrichedAsync(Security security)
        {
            if (security == null)
            {
                return null;
            }

            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .Include(x => x.Price)
                            .FindOne(x => x.Security.SecurityId == security.SecurityId);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(FromSecurityEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}