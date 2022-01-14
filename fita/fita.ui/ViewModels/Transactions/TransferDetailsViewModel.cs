using System;
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
using JetBrains.Annotations;
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

        public virtual Transaction Transaction { get; set; }
        
        public Transaction OtherTransaction { get; set; }
        
        public Account Account { get; set; }

        public LockableCollection<Category> Categories { get; set; } = new();
        
        public LockableCollection<Account> Accounts { get; set; } = new();

        public virtual Account OtherAccount { get; set; }
        
        public virtual decimal? PaymentAmount { get; set; }
        
        public virtual string PaymentCulture { get; set; }
        
        public virtual decimal? DepositAmount { get; set; }
        
        public virtual string DepositCulture { get; set; }

        public virtual bool IsReadOnly => Transaction?.Category is {Group: CategoryGroupEnum.TransfersIn};
        
        public bool Saved { get; private set; }
        
        public CategoryRepoService CategoryRepoService { get; set; }
        
        public AccountRepoService AccountRepoService { get; set; }
        
        public TransactionRepoService TransactionRepoService { get; set; }

        [UsedImplicitly]
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

                if (Transaction.TransferTransactionId != null)
                {
                    OtherTransaction = await TransactionRepoService.DetailsEnrichedAsync(Transaction.TransferTransactionId);
                    OtherAccount = Accounts.SingleOrDefault(x => x.AccountId == OtherTransaction.AccountId);

                    PaymentAmount = IsReadOnly ? OtherTransaction?.Payment : Transaction.Payment;
                    DepositAmount = IsReadOnly ? Transaction?.Deposit : OtherTransaction?.Deposit;
                    PaymentCulture = IsReadOnly ? OtherAccount?.Currency.Culture : Account.Currency.Culture;
                    DepositCulture = IsReadOnly ? Account?.Currency.Culture : OtherAccount?.Currency.Culture;
                }
                else
                {
                    // create new transaction and link both
                    OtherTransaction = new Transaction
                    {
                        TransferTransactionId = Transaction.TransactionId,
                        Date = DateTime.Now
                    };

                    Transaction.Date = DateTime.Now;
                    Transaction.TransferTransactionId = OtherTransaction.TransactionId;
                    PaymentCulture = Account.Currency.Culture;
                }
            }
            finally
            {
                Categories.EndUpdate();
                Accounts.EndUpdate();
                IsBusy = false;
            }

        }

        [UsedImplicitly]
        public void Cancel() => DocumentOwner?.Close(this);

        [UsedImplicitly]
        public async Task Save()
        {
            IsBusy = true;

            try
            {
                if (!IsReadOnly)
                {
                    Transaction.Description = $"Transfer to {OtherAccount.Name}";
                    Transaction.Category = Categories.First(x => x.Group == CategoryGroupEnum.TransfersOut);
                    Transaction.Payment = PaymentAmount;
                    
                    OtherTransaction.AccountId = OtherAccount.AccountId;
                    OtherTransaction.Description = $"Transfer from {Account.Name}";
                    OtherTransaction.Category = Categories.First(x => x.Group == CategoryGroupEnum.TransfersIn);
                    OtherTransaction.Deposit = DepositAmount;
                }
                else
                {
                    OtherTransaction.Payment = PaymentAmount;
                    Transaction.Deposit = DepositAmount;
                }

                OtherTransaction.Notes = Transaction.Notes;

                if (await TransactionRepoService.SaveAsync(Transaction) == Result.Success &&
                    await TransactionRepoService.SaveAsync(OtherTransaction) == Result.Success)
                {
                    Messenger.Default.Send(new NotificationMessage("Transfer saved.", NotificationStatusEnum.Success));
                }
                else
                {
                    Messenger.Default.Send(new NotificationMessage("Transfer failed.", NotificationStatusEnum.Error));
                }

                Saved = true;
                DocumentOwner?.Close(this);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public bool CanSave()
            => OtherAccount is not null && PaymentAmount is not null && DepositAmount is not null;

        [UsedImplicitly]
        protected void OnOtherAccountChanged(Account oldAccount)
        {
            DepositCulture = IsReadOnly ? Account?.Currency.Culture : OtherAccount?.Currency.Culture;
        }
    }
}