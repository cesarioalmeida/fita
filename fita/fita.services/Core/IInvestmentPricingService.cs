using System;
using System.Threading.Tasks;
using fita.data.Models;

namespace fita.services.Core
{
    public interface IInvestmentPricingService
    {
        Task<decimal> GetPrice(Security security, DateTime date);
    }
}