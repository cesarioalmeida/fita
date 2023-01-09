using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.services.Repositories;

[Export]
public class CurrencyRepoService : RepositoryService<Currency>, ISeedData
{
    public CurrencyRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)  
        => IndexData();

    public Task<Result> SeedData()
    {
        return Task.Run(async () =>
        {
            var euro = new Currency
            {
                CurrencyId = ObjectId.NewObjectId(),
                Name = "Euro",
                Symbol = "EUR",
                Culture = "pt-PT"
            };

            if (Collection.FindOne(x => x.Name == euro.Name) is null)
            {
                return await Save(euro);
            }

            return Result.Success;
        });
    }

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.CurrencyId);
        Collection.EnsureIndex(x => x.Name);
    }
}