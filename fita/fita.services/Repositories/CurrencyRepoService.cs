using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class CurrencyRepoService : RepositoryService<Currency>, ISeedData
    {
        public CurrencyRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }

        public Task<Result> SeedData()
        {
            return Task.Run(async () =>
            {
                var euro = new Currency
                {
                    CurrencyId = ObjectId.NewObjectId(),
                    Name = "Euro",
                    Symbol = "EUR",
                    Prefix = "€", 
                    Suffix = string.Empty
                };

                if (Collection.FindOne(x => x.Name == euro.Name) == null)
                {
                    return await SaveAsync(euro);
                };

                return Result.Success;
            });
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.CurrencyId);
            Collection.EnsureIndex(x => x.Name);
        }
    }
}