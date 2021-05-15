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
    public class ClosedPositionsReportViewModel : ComposedViewModelBase
    {
        public LockableCollection<Model> Data { get; set; } = new();

        public AccountRepoService AccountRepoService { get; set; }

        public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

        public ClosedPositionsReportViewModel()
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
                var closedPositions = await ClosedPositionRepoService.AllEnrichedAsync();

                foreach (var position in closedPositions.OrderBy(x => x.SellDate))
                {
                    Data.Add(new Model(position, accounts.Single(x => x.AccountId == position.AccountId)));
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
            public Model(ClosedPosition position, Account account)
            {
                Position = position;
                Account = account;
            }

            public ClosedPosition Position { get; }
            
            public Account Account { get; }
        }
    }
}