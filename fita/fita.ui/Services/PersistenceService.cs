using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fita.core.Models;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.ui.Services
{
    public class PersistenceService
    {
        public IDBHelperService DBHelperService { get; set; }

        public ILoggingService LoggingService { get; set; }

        #region Currency

        public Task<Currency> GetCurrency(int id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        var table = DBHelperService.DB.GetCollection<Currency>();
                        return table.FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(GetCurrency)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<List<Currency>> GetCurrencies()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        var table = DBHelperService.DB.GetCollection<Currency>();
                        return table.FindAll().ToList();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(GetCurrencies)}: {ex}");
                        return null;
                    }
                });
        }

        public Task<bool> SaveCurrency(Currency currency)
        {
            return Task.Run(
                () =>
                {
                    if (currency == null)
                    {
                        return false;
                    }

                    if (currency.HasChanges || currency.SavedState == null)
                    {
                        try
                        {
                            var table = DBHelperService.DB.GetCollection<Currency>();
                            return table.Upsert(currency);
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Warn($"{nameof(SaveCurrency)}: {ex}");
                            return false;
                        }
                    }

                    return false;
                });
        }

        #endregion
    }
}