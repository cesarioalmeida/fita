using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Repositories;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Portfolio
{
    [POCOViewModel]
    public class PortfolioViewModel : ComposedViewModelBase
    {
        public virtual Account Account { get; set; }

        public LockableCollection<EntityModel> Data { get; set; } = new();
        
        public AccountRepoService AccountRepoService { get; set; }
        
        public SecurityPositionRepoService SecurityPositionRepoService { get; set; }
        
        public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }
        
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
                var positions = await SecurityPositionRepoService.AllEnrichedForAccountAsync(Account.AccountId);

                foreach (var position in positions)
                {
                    var latestPrice = (await SecurityHistoryRepoService.FromSecurityEnrichedAsync(position.Security))?
                        .Price?.LatestValue ?? 0m;
                    Data.Add(new EntityModel(position, latestPrice, position.Quantity * latestPrice - position.Value));
                }
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        public void NavigateTo()
        {
            NavigationService?.Navigate("TransactionsView", Account, this);
        }

        protected override async void OnNavigatedTo()
        {
            Account = Parameter as Account;

            await RefreshData();
        }
        
        public class EntityModel
        {
            public EntityModel(SecurityPosition position, decimal currentPrice, decimal pl)
            {
                Entity = position;
                CurrentPrice = currentPrice;
                PL = pl;
            }

            public SecurityPosition Entity { get; }
            
            public decimal CurrentPrice { get; }
            
            public decimal PL { get; }
        }
    }
}