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
        public int Width => 400;

        public int Height => 600;

        public virtual Trade Trade { get; set; }
        
        public virtual Account Account { get; set; }
        
        public LockableCollection<Security> Securities { get; set; } = new();

        public virtual Security Security { get; set; }
        
        public virtual decimal? PaymentAmount { get; set; }
        
        public virtual string PaymentCulture { get; set; }
        
        public virtual decimal? DepositAmount { get; set; }
        
        public virtual string DepositCulture { get; set; }

        public virtual bool IsReadOnly => false;
        
        public bool Saved { get; private set; }
        
        public TransactionRepoService TransactionRepoService { get; set; }
        
        public SecurityRepoService SecurityRepoService { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;
            
            Securities.BeginUpdate();
            Securities.Clear();
            
            try
            {
                var securities = await SecurityRepoService.AllEnrichedAsync();
                Securities.AddRange(securities);
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
                // if (OtherAccount == null || PaymentAmount == null || DepositAmount == null)
                // {
                //     DocumentOwner?.Close(this);
                //     return;
                // }
                //
                // if (!IsReadOnly)
                // {
                //     Transaction.Description = $"Transfer to {OtherAccount.Name}";
                //     Transaction.Category = Categories.First(x => x.Group == CategoryGroupEnum.TransfersOut);
                //     Transaction.Payment = PaymentAmount;
                //     
                //     OtherTransaction.AccountId = OtherAccount.AccountId;
                //     OtherTransaction.Description = $"Transfer from {Account.Name}";
                //     OtherTransaction.Category = Categories.First(x => x.Group == CategoryGroupEnum.TransfersIn);
                //     OtherTransaction.Deposit = DepositAmount;
                // }
                // else
                // {
                //     OtherTransaction.Payment = PaymentAmount;
                //     Transaction.Deposit = DepositAmount;
                // }
                //
                // OtherTransaction.Date = Transaction.Date;
                // OtherTransaction.Notes = Transaction.Notes;
                //
                // if (await TransactionRepoService.SaveAsync(Transaction) == Result.Success &&
                //     await TransactionRepoService.SaveAsync(OtherTransaction) == Result.Success)
                // {
                //     Messenger.Default.Send(new NotificationMessage("Transfer saved.", NotificationStatusEnum.Success));
                // }
                // else
                // {
                //     Messenger.Default.Send(new NotificationMessage("Transfer failed.", NotificationStatusEnum.Error));
                // }

                Saved = true;
                DocumentOwner?.Close(this);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}