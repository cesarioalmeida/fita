using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class ExchangeRateService : RepositoryService<ExchangeRate>
    {
        public ExchangeRateService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(dbHelperService, loggingService)
        {
            IndexData();
        }
        
        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.ExchangeRateId);
        }
        
        public override Task<ExchangeRate> DetailsEnrichedAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.FromCurrency)
                            .Include(x => x.ToCurrency)
                            .Include(x => x.HistoricalData)
                            .FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public override Task<IEnumerable<ExchangeRate>> AllEnrichedAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection
                            .Include(x => x.FromCurrency)
                            .Include(x => x.ToCurrency)
                            .Include(x => x.HistoricalData)
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