using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class ClosedPositionRepoService : RepositoryService<ClosedPosition>
{
    public ClosedPositionRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)  
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.ClosedPositionId);
        Collection.EnsureIndex(x => x.Security);
    }
        
    public Task<IEnumerable<ClosedPosition>> AllEnrichedForSecurity(ObjectId securityId)
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
                    LoggingService.Warn($"{nameof(AllEnrichedForSecurity)}: {ex}");
                    return null;
                }
            });
    }

    public Task<IEnumerable<ClosedPosition>> AllEnrichedBetweenDates(DateTime startDate, DateTime? endDate = null)
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
                    LoggingService.Warn($"{nameof(AllEnrichedBetweenDates)}: {ex}");
                    return null;
                }
            });
    }
        
    public Task<IEnumerable<ClosedPosition>> AllEnrichedToDate(DateTime endDate)
    {
        return Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.Security)
                        .Find(x => x.SellDate <= endDate);
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(AllEnrichedToDate)}: {ex}");
                    return null;
                }
            });
    }
}