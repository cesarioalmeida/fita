using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Repositories;
using fita.ui.Messages;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class AllTransactionsReportViewModel : ComposedViewModelBase
    {
        public LockableCollection<Model> Data { get; set; } = new();

        public AccountRepoService AccountRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public AllTransactionsReportViewModel()
        {
            Messenger.Default.Register<BaseCurrencyChanged>(this, _ => { RefreshData(); });
            Messenger.Default.Register<AccountsChanged>(this, _ => { RefreshData(); });
        }

        public async Task RefreshData()
        {
            IsBusy = true;
            Data.BeginUpdate();

            try
            {
                Data.Clear();

                var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();
                var transactions = await TransactionRepoService.AllEnrichedAsync();

                foreach (var transaction in transactions.OrderBy(x => x.Date))
                {
                    Data.Add(new Model(transaction, accounts.Single(x => x.AccountId == transaction.AccountId)));
                }
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        public class Model
        {
            public Model(Transaction transaction, Account account)
            {
                Transaction = transaction;
                Account = account;
            }

            public Transaction Transaction { get; }
            
            public Account Account { get; }
        }
    }
}