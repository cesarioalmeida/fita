using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Views.Transactions;
using twentySix.Framework.Core.Extensions;
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
                
                _transactions = (await TransactionRepoService.AllEnrichedForAccountAsync(Account?.AccountId)).ToList();

                Data.Clear();

                var balance = 0m;

                Data.Add(EntityModel.GetInitialBalance(Account));

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
            var viewModel = ViewModelSource.Create<TransactionDetailsViewModel>();
            viewModel.Entity = model.Entity ?? new Transaction { AccountId = Account.AccountId };
            viewModel.Account = model.Account;

            var document = this.ModalDocumentManagerService.CreateDocument(nameof(TransactionDetailsView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                fireChangeNotification = true;

                await RefreshData();
            }
        }

        protected override async void OnNavigatedTo()
        {
            Account = Parameter as Account;

            await RefreshData();
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