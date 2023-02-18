using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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

    public async Task<IEnumerable<ExchangeRate>> GetAllFromCurrency(Currency fromCurrency)
    {
        if (fromCurrency is null)
        {
            return null;
        }

        try
        {
            return (await GetAllConditional(x => x.FromCurrency.CurrencyId == fromCurrency.CurrencyId, true));
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllFromCurrency)}: {ex}");
            return null;
        }
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllWithCurrency(Currency currency)
    {
        if (currency is null)
        {
            return null;
        }

        try
        {
            return (await GetAllConditional(x => x.FromCurrency.CurrencyId == currency.CurrencyId ||
                                                 x.ToCurrency.CurrencyId == currency.CurrencyId, true));
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetAllWithCurrency)}: {ex}");
            return null;
        }
    }

    public async Task<ExchangeRate> GetSingleFromToCurrency(Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency is null || toCurrency is null)
        {
            return null;
        }

        try
        {
            return (await GetAllConditional(x => x.FromCurrency.CurrencyId == fromCurrency.CurrencyId &&
                                                 x.ToCurrency.CurrencyId == toCurrency.CurrencyId, true)).Single();
        }
        catch (Exception ex)
        {
            LoggingService.Warn($"{nameof(GetSingleFromToCurrency)}: {ex}");
            return null;
        }
    }
}