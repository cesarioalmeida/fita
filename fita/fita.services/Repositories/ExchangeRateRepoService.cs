﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class ExchangeRateRepoService : RepositoryService<ExchangeRate>
    {
        public ExchangeRateRepoService(IDBHelperService dbHelperService, ILoggingService loggingService) : base(
            dbHelperService, loggingService)
        {
            IndexData();
        }

        public sealed override void IndexData()
        {
            Collection.EnsureIndex(x => x.ExchangeRateId);
            Collection.EnsureIndex(x => x.FromCurrency);
            Collection.EnsureIndex(x => x.ToCurrency);
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
                            .Include(x => x.Rate)
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
                            .Include(x => x.Rate)
                            .FindAll();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<IEnumerable<ExchangeRate>> AllFromCurrencyEnrichedAsync(Currency fromCurrency)
        {
            if (fromCurrency == null)
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
                        LoggingService.Warn($"{nameof(AllFromCurrencyEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<IEnumerable<ExchangeRate>> AllWithCurrencyEnrichedAsync(Currency currency)
        {
            if (currency == null)
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
                        LoggingService.Warn($"{nameof(AllWithCurrencyEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<ExchangeRate> FromToCurrencyEnrichedAsync(Currency fromCurrency, Currency toCurrency)
        {
            if (fromCurrency == null || toCurrency == null)
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
                        LoggingService.Warn($"{nameof(FromToCurrencyEnrichedAsync)}: {ex}");
                        return null;
                    }
                });
        }
    }
}