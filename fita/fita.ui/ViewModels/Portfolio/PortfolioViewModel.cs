using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Portfolio
{
    [POCOViewModel]
    public class PortfolioViewModel : ComposedViewModelBase
    {
        private List<Transaction> _transactions { get; set; }
        
        public virtual Account Account { get; set; }

        public ObservableCollection<EntityModel> Data { get; set; } = new();
        
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