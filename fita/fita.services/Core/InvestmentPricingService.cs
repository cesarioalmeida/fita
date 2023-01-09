using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using fita.data.Models;
using fita.services.Repositories;

namespace fita.services.Core;

[Export(typeof(IInvestmentPricingService))]
public class InvestmentPricingService : IInvestmentPricingService
{
    [Import]
    public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }
    
    public async Task<decimal> GetPrice(Security security, DateTime date)
    {
        var existingData = await SecurityHistoryRepoService.GetFromSecurity(security);

        return existingData.Price.DataPoints.LastOrDefault(x => x.Date <= date)?.Value ??
               existingData.Price.DataPoints.First().Value;
    }
}