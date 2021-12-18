using System;
using System.Threading.Tasks;
using fita.data.Models;

namespace fita.services.Core
{
    public interface IExchangeRateService
    {
        Task<Result> Update(ExchangeRate exchangeRate, DateTime? date = null);

        Task<decimal> Exchange(Currency fromCurrency, Currency toCurrency, decimal value, DateTime? date = null);
    }
}