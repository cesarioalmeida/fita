using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Models;
using fita.services;
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
        private List<Category> _categories { get; set; } = new();

        public int Width => 400;

        public int Height => 600;

        public Trade Trade { get; set; }
        
        public Transaction Transaction { get; set; }

        public Account Account { get; set; }

        public LockableCollection<Security> Securities { get; set; } = new();

        public virtual Security Security { get; set; }
        
        public virtual bool IsReadOnly => false;

        public bool Saved { get; private set; }

        public TradeRepoService TradeRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public CategoryRepoService CategoryRepoService { get; set; }

        public SecurityRepoService SecurityRepoService { get; set; }

        public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }

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

                if (!_categories.Any())
                {
                    _categories.AddRange(await CategoryRepoService.AllAsync());
                }
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

                Transaction ??= new Transaction();
                Transaction.AccountId = Trade.AccountId;
                Transaction.TradeId = Trade.TradeId;
                Transaction.Date = Trade.Date;
                Transaction.Description = $"Trade {Trade.Security.Name}: " +
                                          (Trade.Action == TradeActionEnum.Buy ? "Bought" : "Sold") +
                                          $" {Trade.Quantity:F0} @ {Trade.Price:f4}";
                Transaction.Category = _categories.Single(x =>
                    x.Group == (Trade.Action == TradeActionEnum.Buy
                        ? CategoryGroupEnum.TradeBuy
                        : CategoryGroupEnum.TradeSell));
                Transaction.Notes = Trade.Security.Name;
                Transaction.Payment = Trade.Action == TradeActionEnum.Buy ? Trade.Value : null;
                Transaction.Deposit = Trade.Action == TradeActionEnum.Sell ? Trade.Value : null;

                if (await TradeRepoService.SaveAsync(Trade) == Result.Success &&
                    await TransactionRepoService.SaveAsync(Transaction) == Result.Success)
                {
                    Messenger.Default.Send(new NotificationMessage("Trade saved.", NotificationStatusEnum.Success));
                }
                else
                {
                    Messenger.Default.Send(new NotificationMessage("Trade failed.", NotificationStatusEnum.Error));
                }

                Saved = true;
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