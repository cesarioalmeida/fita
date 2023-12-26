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

    public async Task<IEnumerable<ClosedPosition>> GetAllForSecurity(ObjectId securityId)
    {
        try
        {
            return (await GetAllConditional(x => x.Security.SecurityId == securityId, true));
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllForSecurity)}: {ex}");
            return null;
        }
    }

    public async Task<IEnumerable<ClosedPosition>> GetAllBetweenDates(DateTime startDate, DateTime? endDate = null)
    {
        try
        {
            endDate ??= DateTime.MaxValue;

            return (await GetAllConditional(x => x.SellDate >= startDate && x.SellDate <= endDate, true));
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllBetweenDates)}: {ex}");
            return null;
        }
    }
}