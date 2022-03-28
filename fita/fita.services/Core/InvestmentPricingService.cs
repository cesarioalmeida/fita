using System;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;

namespace fita.services.Core;

public class InvestmentPricingService : IInvestmentPricingService
{
    public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }
    
    public async Task<decimal> GetPrice(Security security, DateTime date)
    {
        var existingData = await SecurityHistoryRepoService.FromSecurityEnrichedAsync(security);

        return existingData.Price.DataPoints.LastOrDefault(x => x.Date <= date)?.Value ??
               existingData.Price.DataPoints.First().Value;
    }
}