using System.Collections.Generic;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Repositories;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Portfolio
{
    [POCOViewModel]
    public class PortfolioViewModel : ComposedViewModelBase
    {
        private List<Transaction> _transactions { get; set; }
        
        public virtual Account Account { get; set; }

        public LockableCollection<SecurityPosition> Data { get; set; } = new();
        
        public AccountRepoService AccountRepoService { get; set; }
        
        public SecurityPositionRepoService SecurityPositionRepoService { get; set; }
        
        public async Task RefreshData()
        {
            IsBusy = true;
            
            if (Account == null)
            {
                return;
            }
                
            Account = await AccountRepoService.DetailsEnrichedAsync(Account.AccountId);
            if (Account == null)
            {
                return;
            }
            
            Data.BeginUpdate();
            
            try
            {
                Data.Clear();
                Data.AddRange(await SecurityPositionRepoService.AllEnrichedForAccountAsync(Account.AccountId));
            }
            finally
            {
                Data.EndUpdate();
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
            }

            public Transaction Entity { get; }
            
            public Account Account { get; }
        }
    }
}