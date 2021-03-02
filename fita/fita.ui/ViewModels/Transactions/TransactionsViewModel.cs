using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.WindowsUI;
using fita.data.Enums;
using fita.data.Models;
using fita.services;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Views.Transactions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions
{
    [POCOViewModel]
    public class TransactionsViewModel : ComposedViewModelBase
    {
        private bool fireChangeNotification;
        
        private List<Transaction> _transactions { get; set; }
        
        public virtual Account Account { get; set; }

        public ObservableCollection<EntityModel> Data { get; set; } = new();
        
        public AccountRepoService AccountRepoService { get; set; }
        
        public TransactionRepoService TransactionRepoService { get; set; }

        public IAccountService AccountService { get; set; }
        
        protected IDocumentManagerService ModalDocumentManagerService =>
            this.GetRequiredService<IDocumentManagerService>("ModalWindowDocumentService");

        public async Task RefreshData()
        {
            IsBusy = true;

            try
            {
                if (Account == null)
                {
                    return;
                }
                
                // refresh account
                Account = await AccountRepoService.DetailsEnrichedAsync(Account.AccountId);
                if (Account == null)
                {
                    return;
                }
                
                _transactions = (await TransactionRepoService.AllEnrichedForAccountAsync(Account?.AccountId)).ToList();

                Data.Clear();

                Data.Add(EntityModel.GetInitialBalance(Account));
                
                var balance = Account.InitialBalance;

                foreach (var transaction in _transactions)
                {
                    balance = await AccountService.CalculateBalance(transaction, balance);
                    Data.Add(new EntityModel(Account, transaction, balance));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task Edit(EntityModel model)
        {
            var detailsSaved = model.Entity.Category.Group == CategoryGroupEnum.TransfersIn ||
                               model.Entity.Category.Group == CategoryGroupEnum.TransfersOut
                ? EditTransfer(model)
                : EditTransaction(model);

            if (detailsSaved)
            {
                fireChangeNotification = true;

                await RefreshData();
            }
        }

        public bool CanEdit(EntityModel model)
        {
            return model?.Entity.Category != null;
        }
        
        public async Task NewTransaction()
        {
            var detailsSaved = EditTransaction(null);
            
            if (detailsSaved)
            {
                fireChangeNotification = true;

                await RefreshData();
            }
        }
        
        public async Task NewTransfer()
        {
            var detailsSaved = EditTransfer(null);
            
            if (detailsSaved)
            {
                fireChangeNotification = true;

                await RefreshData();
            }
        }
        
        public async Task Delete(EntityModel model)
        {
            if (model == null)
            {
                return;
            }

            if (WinUIMessageBox.Show(
                $"Are you sure you want to delete the transaction {model.Entity}?",
                "Delete transaction",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            IsBusy = true;

            try
            {
                if (model.Entity.AssociatedTransactionId != null)
                {
                    await TransactionRepoService.DeleteAsync(model.Entity.AssociatedTransactionId);
                }
                
                Messenger.Default.Send(await TransactionRepoService.DeleteAsync(model.Entity.TransactionId) == Result.Fail
                    ? new NotificationMessage("Failed to delete transaction.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Transaction {model.Entity} deleted.", NotificationStatusEnum.Success));

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool CanDelete(EntityModel model)
        {
            return model?.Entity.Category != null;
        }
        
        protected override async void OnNavigatedTo()
        {
            Account = Parameter as Account;

            await RefreshData();
        }
        
        private bool EditTransfer(EntityModel model)
        {
            var viewModel = ViewModelSource.Create<TransferDetailsViewModel>();
            viewModel.Transaction = model?.Entity ?? new Transaction { AccountId = Account.AccountId };
            viewModel.Account = Account;
            
            var document = this.ModalDocumentManagerService.CreateDocument(nameof(TransferDetailsView), viewModel, null, this);
            
            document.DestroyOnClose = true;
            document.Show();

            return viewModel.Saved;
        }
        
        private bool EditTransaction(EntityModel model)
        {
            var viewModel = ViewModelSource.Create<TransactionDetailsViewModel>();
            viewModel.Entity = model?.Entity ?? new Transaction { AccountId = Account.AccountId };
            viewModel.Account = Account;
            
            var document = this.ModalDocumentManagerService.CreateDocument(nameof(TransactionDetailsView), viewModel, null, this);
            
            document.DestroyOnClose = true;
            document.Show();

            return viewModel.Saved;
        }

        public class EntityModel
        {
            public EntityModel(Account account, Transaction transaction, decimal balance)
            {
                Entity = transaction;
                Account = account;
                Balance = balance;
            }

            public Transaction Entity { get; }
            
            public Account Account { get; }
            
            public decimal? Balance { get; }

            public static EntityModel GetInitialBalance(Account account)
            {
                return new(account, new Transaction
                    {AccountId = account.AccountId, Description = "Initial balance", Deposit = account.InitialBalance}, account.InitialBalance);
            }
        }
    }
}