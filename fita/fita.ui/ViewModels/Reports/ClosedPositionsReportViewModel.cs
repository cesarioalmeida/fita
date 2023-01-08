using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using JetBrains.Annotations;

namespace fita.ui.ViewModels.Reports;

[POCOViewModel]
public class ClosedPositionsReportViewModel : ReportBaseViewModel
{
    public LockableCollection<Model> Data { get; set; } = new();
        
    [Import]
    public AccountRepoService AccountRepoService { get; set; }

    [Import]
    public ClosedPositionRepoService ClosedPositionRepoService { get; set; }
        
    [Import]
    public FileSettingsRepoService FileSettingsRepoService { get; set; }
        
    [Import]
    public IExchangeRateService ExchangeRateService { get; set; }
        
    public override async Task RefreshData()
    {
        IsBusy = true;
        BaseCurrency = (await FileSettingsRepoService.GetAll(true)).First().BaseCurrency;
        Data.BeginUpdate();

        try
        {
            Data.Clear();

            var accounts = (await AccountRepoService.GetAll(true)).ToList();
            var closedPositions = await ClosedPositionRepoService.GetAll(true);

            foreach (var position in closedPositions.OrderBy(x => x.SellDate))
            {
                var account = accounts.Single(x => x.AccountId == position.AccountId);
                var profitLoss = await ExchangeRateService.Exchange(account.Currency, BaseCurrency, position.ProfitLoss);
                    
                Data.Add(new(position, account, profitLoss, BaseCurrency.Culture));
            }
        }
        finally
        {
            Data.EndUpdate();
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public record Model(ClosedPosition Position, Account Account, decimal ProfitLossBaseCurrency, string BaseCulture);
}