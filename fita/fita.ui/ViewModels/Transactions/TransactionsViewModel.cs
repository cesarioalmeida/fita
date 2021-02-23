using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using fita.data.Models;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions
{
    [POCOViewModel]
    public class TransactionsViewModel : ComposedViewModelBase
    {
        public virtual Account Account { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;

            try
            {
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override void OnNavigatedTo()
        {
            Account = Parameter as Account;
        }
    }
}