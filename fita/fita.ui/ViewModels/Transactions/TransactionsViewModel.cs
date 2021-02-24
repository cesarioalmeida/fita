using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions
{
    [POCOViewModel]
    public class TransactionsViewModel : ComposedViewModelBase
    {
        private List<Transaction> _transactions { get; set; }

        public virtual Account Account { get; set; }

        public virtual ObservableCollection<Category> Categories { get; set; } = new();

        public ObservableCollection<EntityModel> Data { get; set; } = new();

        public virtual EntityModel SelectedData { get; set; }

        public CategoryRepoService CategoryRepoService { get; set; }

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

                Categories.Clear();
                Categories.AddRange(await CategoryRepoService.AllAsync());

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

        protected override async void OnNavigatedTo()
        {
            Account = Parameter as Account;

            await RefreshData();
        }

        public class EntityModel
        {
            private readonly Account _account;
            private Transaction _transaction;

            public EntityModel(Account account, Transaction transaction, decimal balance)
            {
                _account = account;
                _transaction = transaction;

                Date = _transaction.Date;
                Description = _transaction.Description;
                Notes = _transaction.Notes;
                Category = _transaction.Category;
                Payment = _transaction.Payment;
                Deposit = _transaction.Deposit;
                TransferToAccount = _transaction.TransferAccount;
                Balance = balance;
            }

            public DateTime? Date { get; set; }

            public string Description { get; set; }

            public string Notes { get; set; }

            public Category Category { get; set; }

            public decimal? Payment { get; set; }

            public decimal? Deposit { get; set; }

            public Account TransferToAccount { get; set; }

            public decimal? Balance { get; set; }

            public static EntityModel GetInitialBalance(Account account)
            {
                return new(account, new Transaction
                    {Description = "Initial balance", Deposit = account.InitialBalance}, account.InitialBalance);
            }

            public Transaction ToTransaction()
            {
                _transaction ??= new Transaction {AccountId = _account.AccountId};

                _transaction.Date = Date.GetValueOrDefault();
                _transaction.Description = Description;
                _transaction.Notes = Notes;
                _transaction.Category = Category;
                _transaction.Payment = Payment;
                _transaction.Deposit = Deposit;
                _transaction.TransferAccount = TransferToAccount;

                return _transaction;
            }
        }
    }
}