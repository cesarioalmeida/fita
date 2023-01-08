using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories;

[Export]
public class ExchangeRateRepoService : RepositoryService<ExchangeRate>
{
    public ExchangeRateRepoService([Import] DBHelperServiceFactory dbHelperServiceFactory,
        [Import] ILoggingService loggingService)
        : base(dbHelperServiceFactory.GetInstance(), loggingService)
        => IndexData();

    public sealed override void IndexData()
    {
        Collection.EnsureIndex(x => x.ExchangeRateId);
        Collection.EnsureIndex(x => x.FromCurrency);
        Collection.EnsureIndex(x => x.ToCurrency);
    }
        
    public Task<IEnumerable<ExchangeRate>> AllFromCurrencyEnriched(Currency fromCurrency)
    {
        if (fromCurrency is null)
        {
            return null;
        }

        return Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.FromCurrency)
                        .Include(x => x.ToCurrency)
                        .Include(x => x.Rate)
                        .Find(x => x.FromCurrency.CurrencyId == fromCurrency.CurrencyId);
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(AllFromCurrencyEnriched)}: {ex}");
                    return null;
                }
            });
    }

    public Task<IEnumerable<ExchangeRate>> AllWithCurrencyEnriched(Currency currency)
    {
        if (currency is null)
        {
            return null;
        }

        return Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.FromCurrency)
                        .Include(x => x.ToCurrency)
                        .Include(x => x.Rate)
                        .Find(x => x.FromCurrency.CurrencyId == currency.CurrencyId ||
                                   x.ToCurrency.CurrencyId == currency.CurrencyId);
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(AllWithCurrencyEnriched)}: {ex}");
                    return null;
                }
            });
    }

    public Task<ExchangeRate> FromToCurrencyEnriched(Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency is null || toCurrency is null)
        {
            return null;
        }

        return Task.Run(
            () =>
            {
                try
                {
                    return Collection
                        .Include(x => x.FromCurrency)
                        .Include(x => x.ToCurrency)
                        .Include(x => x.Rate)
                        .FindOne(x => x.FromCurrency.CurrencyId == fromCurrency.CurrencyId &&
                                      x.ToCurrency.CurrencyId == toCurrency.CurrencyId);
                }
                catch (Exception ex)
                {
                    LoggingService.Warn($"{nameof(FromToCurrencyEnriched)}: {ex}");
                    return null;
                }
            });
    }
}