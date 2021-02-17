using System.Threading.Tasks;
using fita.data.Models;

namespace fita.services.Core
{
    public interface IExchangeRateService
    {
        Task<Result> UpdateAsync(ExchangeRate exchangeRate);

        Task<decimal> Exchange(Currency fromCurrency, Currency toCurrency, decimal value);
    }
}