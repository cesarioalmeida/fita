using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class ClosedPositionRepoService : RepositoryService<ClosedPosition>
    {
        public ClosedPositionRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.ClosedPositionId);
            Collection.EnsureIndex(x => x.Security);
        }

        public override Task<ClosedPosition> DetailsEnrichedAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public override Task<IEnumerable<ClosedPosition>> AllEnrichedAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .FindAll();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }
        
        public Task<IEnumerable<ClosedPosition>> AllEnrichedForSecurityAsync(ObjectId securityId)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.Security)
                            .Find(x => x.Security.SecurityId == securityId);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedForSecurityAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<IEnumerable<ClosedPosition>> AllEnrichedBetweenDatesAsync(DateTime startDate, DateTime? endDate = null)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        endDate ??= DateTime.MaxValue;

                        return Collection
                            .Include(x => x.Security)
                            .Find(x => x.SellDate >= startDate && x.SellDate <= endDate);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedBetweenDatesAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}