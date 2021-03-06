using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Common;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions
{
    [POCOViewModel]
    public class TradeDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
    {
        public int Width => 400;

        public int Height => 600;

        public Trade Trade { get; set; }
        
        public Transaction Transaction { get; set; }

        public Account Account { get; set; }

        public LockableCollection<Security> Securities { get; set; } = new();

        public virtual Security Security { get; set; }
        
        public bool Saved { get; private set; }

        public SecurityRepoService SecurityRepoService { get; set; }

        public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }
        
        public IPortfolioService PortfolioService { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;

            Securities.BeginUpdate();
            Securities.Clear();

            try
            {
                var securities = await SecurityRepoService.AllEnrichedAsync();
                Securities.AddRange(securities);

                Security = Securities.SingleOrDefault(x => x.SecurityId == Trade.Security?.SecurityId);
            }
            finally
            {
                Securities.EndUpdate();
                IsBusy = false;
            }
        }

        public void Cancel()
        {
            DocumentOwner?.Close(this);
        }

        public async Task Save()
        {
            IsBusy = true;

            try
            {
                Trade.Security = Security;

                if (await PortfolioService.IsTradePossible(Trade) && await PortfolioService.ProcessTrade(Trade, Transaction))
                {
                    Messenger.Default.Send(new NotificationMessage("Trade saved.", NotificationStatusEnum.Success));
                    Saved = true;
                }
                else
                {
                    Messenger.Default.Send(new NotificationMessage("Trade failed.", NotificationStatusEnum.Error));
                }
                
                DocumentOwner?.Close(this);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool CanSave()
        {
            return Trade.Quantity != 0m && Trade.Price > 0m;
        }

        protected async void OnSecurityChanged(Security oldValue)
        {
            if (Trade == null || Account == null || Security == null)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Trade.Price =
                    (await SecurityHistoryRepoService.FromSecurityEnrichedAsync(Security))?.Price?.LatestValue ?? 0m;
                RaisePropertyChanged(() => Trade);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}