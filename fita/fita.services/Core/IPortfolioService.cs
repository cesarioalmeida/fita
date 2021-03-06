using System.Collections.Generic;
using System.Threading.Tasks;
using fita.data.Models;

namespace fita.services.Core
{
    public interface IPortfolioService
    {
        Task<bool> IsTradePossible(Trade trade);

        Task<bool> ProcessTrade(Trade trade, Transaction transaction);
    }
}