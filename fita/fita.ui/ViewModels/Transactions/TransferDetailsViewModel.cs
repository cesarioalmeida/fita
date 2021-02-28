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
    public class TransferDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
    {
        public int Width => 400;

        public int Height => 600;

        public virtual Transaction Entity { get; set; }
        
        public Transaction AssociatedTransaction { get; set; }
        
        public Account Account { get; set; }

        public LockableCollection<Category> Categories { get; set; } = new();
        
        public LockableCollection<Account> Accounts { get; set; } = new();

        public virtual Account SelectedAccount { get; set; }
        
        public virtual decimal? OtherAccountAmount { get; set; }

        public bool Saved { get; private set; }
        
        public CategoryRepoService CategoryRepoService { get; set; }
        
        public AccountRepoService AccountRepoService { get; set; }
        
        public TransactionRepoService TransactionRepoService { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;

            Categories.BeginUpdate();
            Categories.Clear();
            Accounts.BeginUpdate();
            Accounts.Clear();
            try
            {
                var categories = await CategoryRepoService.AllAsync();
                Categories.AddRange(categories);
                
                var accounts = await AccountRepoService.AllEnrichedAsync();
                Accounts.AddRange(accounts.Where(x => x.AccountId != Account.AccountId));

                if (Entity.AssociatedTransactionId != null)
                {
                    AssociatedTransaction = await TransactionRepoService.DetailsEnrichedAsync(Entity.AssociatedTransactionId);
                    OtherAccountAmount = AssociatedTransaction?.Category.Group == CategoryGroupEnum.TransfersIn
                        ? AssociatedTransaction?.Deposit
                        : AssociatedTransaction?.Payment;
                    
                    SelectedAccount = Accounts.SingleOrDefault(x => x.AccountId == AssociatedTransaction.AccountId);
                }
                else
                {
                    // create new transaction and link both
                    AssociatedTransaction = new Transaction
                    {
                        AssociatedTransactionId = Entity.TransactionId
                    };

                    Entity.AssociatedTransactionId = AssociatedTransaction.TransactionId;

                    SelectedAccount = Accounts.FirstOrDefault();
                }
            }
            finally
            {
                Categories.EndUpdate();
                Accounts.EndUpdate();
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
                if (SelectedAccount == null || Entity.Payment == null || OtherAccountAmount == null)
                {
                    Saved = true;
                    DocumentOwner?.Close(this);
                    return;
                }
                
                Entity.Description = $"Transfer to {SelectedAccount.Name}";
                Entity.Category = Categories.First(x => x.Group == CategoryGroupEnum.TransfersOut);

                AssociatedTransaction.AccountId = SelectedAccount.AccountId;
                AssociatedTransaction.Date = Entity.Date;
                AssociatedTransaction.Description = $"Transfer from {Account.Name}";
                AssociatedTransaction.Category = Categories.First(x => x.Group == CategoryGroupEnum.TransfersIn);
                AssociatedTransaction.Notes = Entity.Notes;
                AssociatedTransaction.Deposit = OtherAccountAmount;

                if (await TransactionRepoService.SaveAsync(Entity) == Result.Success &&
                    await TransactionRepoService.SaveAsync(AssociatedTransaction) == Result.Success)
                {
                    Messenger.Default.Send(new NotificationMessage($"Transfer saved.", NotificationStatusEnum.Success));
                }
                else
                {
                    Messenger.Default.Send(new NotificationMessage($"Transfer failed.", NotificationStatusEnum.Error));
                }

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