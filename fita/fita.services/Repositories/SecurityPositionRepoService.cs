﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class SecurityPositionRepoService : RepositoryService<SecurityPosition>
{
    public SecurityPositionRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.SecurityPositionId);
        Collection.EnsureIndex(x => x.Security);
        Collection.EnsureIndex(x => x.AccountId);
        Collection.EnsureIndex(x => x.Quantity);
    }

    public async Task<IEnumerable<SecurityPosition>> GetAllForAccount(ObjectId accountId)
    {
        try
        {
            return await GetAllConditional(x => x.AccountId == accountId && x.Quantity > 0, true);
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllForAccount)}: {ex}");
            return null;
        }
    }

    public async Task<SecurityPosition> GetSingleForSecurity(ObjectId accountId, ObjectId securityId)
    {
        try
        {
            return (await GetAllConditional(x => x.AccountId == accountId && x.Security.SecurityId == securityId, true)).SingleOrDefault();
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetSingleForSecurity)}: {ex}");
            return null;
        }
    }
}