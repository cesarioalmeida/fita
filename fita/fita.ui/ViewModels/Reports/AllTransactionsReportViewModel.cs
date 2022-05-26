using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using JetBrains.Annotations;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class AllTransactionsReportViewModel : ReportBaseViewModel
    {
        public LockableCollection<Model> Data { get; set; } = new();
        
        public AccountRepoService AccountRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }
        
        public FileSettingsRepoService FileSettingsRepoService { get; set; }
        
        public IExchangeRateService ExchangeRateService { get; set; }

        public override async Task RefreshData()
        {
            IsBusy = true;
            BaseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;
            Data.BeginUpdate();

            try
            {
                Data.Clear();

                var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();
                var transactions = await TransactionRepoService.AllEnrichedAsync();

                foreach (var transaction in transactions.OrderBy(x => x.Date))
                {
                    var account = accounts.Single(x => x.AccountId == transaction.AccountId);
                    var payment = await ExchangeRateService.Exchange(account.Currency, BaseCurrency, transaction.Payment.GetValueOrDefault());
                    var deposit = await ExchangeRateService.Exchange(account.Currency, BaseCurrency, transaction.Deposit.GetValueOrDefault());
                    
                    Data.Add(new Model(transaction, account, payment, deposit, BaseCurrency.Culture));
                }
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public record Model(Transaction Transaction, Account Account, decimal PaymentBaseCurrency, decimal DepositBaseCurrency, string BaseCulture);
    }
}