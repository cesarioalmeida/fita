using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using LiteDB;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions
{
    [POCOViewModel]
    public class TransactionsViewModel : ComposedViewModelBase
    {
        private List<Transaction> _transactions { get; set; }
        
        public virtual Account Account { get; set; }

        public virtual LockableCollection<Account> Accounts { get; set; } = new();
        
        public virtual LockableCollection<Category> Categories { get; set; } = new();

        public ObservableCollection<EntityModel> Data { get; set; } = new();

        public virtual EntityModel SelectedData { get; set; }

        public CategoryRepoService CategoryRepoService { get; set; }
        
        public AccountRepoService AccountRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public IAccountService AccountService { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;

            try
            {
                if (Account == null)
                {
                    return;
                }
                
                Categories.BeginUpdate();
                Accounts.BeginUpdate();
                
                Categories.Clear();
                Categories.AddRange(await CategoryRepoService.AllAsync());
                
                Accounts.Clear();
                Accounts.AddRange((await AccountRepoService.AllAsync()).Where(x => x.AccountId != Account.AccountId));

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
                Categories.EndUpdate();
                Accounts.EndUpdate();
                
                IsBusy = false;
            }
        }

        protected override async void OnNavigatedTo()
        {
            Account = Parameter as Account;

            await RefreshData();
        }
        
        private async void OnTransactionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = e.OldItems;
            var newItems = e.NewItems;
            
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newItems != null)
                    { 
                        foreach (EntityModel item in newItems)
                        {
                            _transactions.Add(EntityModel.ToTransaction(item));
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await RefreshData();
        }

        public class EntityModel
        {
            private Transaction _transaction;

            public EntityModel(Account account, Transaction transaction, decimal balance)
            {
                _transaction = transaction;

                Account = account;
                
                Date = _transaction.Date;
                Description = _transaction.Description;
                Notes = _transaction.Notes;
                Category = _transaction.Category;
                Payment = _transaction.Payment;
                Deposit = _transaction.Deposit;
                AssociatedTransactionId = _transaction.AssociatedTransactionId;
                Balance = balance;
            }

            public EntityModel()
            {
            }

            public Account Account { get; set; }
            
            public DateTime? Date { get; set; }

            public string Description { get; set; }

            public string Notes { get; set; }

            public Category Category { get; set; }

            public decimal? Payment { get; set; }

            public decimal? Deposit { get; set; }

            public ObjectId AssociatedTransactionId { get; set; }

            public decimal? Balance { get; set; }

            public static EntityModel GetInitialBalance(Account account)
            {
                return new(account, new Transaction
                    {Description = "Initial balance", Deposit = account.InitialBalance}, account.InitialBalance);
            }

            public static Transaction ToTransaction(EntityModel model)
            {
                return new Transaction
                {
                    AccountId = model.Account.AccountId,
                    Date = model.Date.GetValueOrDefault(),
                    Description = model.Description,
                    Notes = model.Notes,
                    Category = model.Category,
                    Payment = model.Payment,
                    Deposit = model.Deposit,
                    AssociatedTransactionId = model.AssociatedTransactionId
                };
            }
        }
    }
}