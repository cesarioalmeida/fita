using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Home
{
    [POCOViewModel]
    public class BanksViewModel : ComposedViewModelBase
    {
        public virtual LockableCollection<EntityModel> Data { get; set; } = new();

        public virtual decimal TotalAmount { get; set; }

        public virtual string BaseCulture {get; set;}

        public AccountRepoService AccountRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

        public IExchangeRateService ExchangeRateService { get; set; }

        public IAccountService AccountService { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;

            Data.BeginUpdate();
            Data.Clear();

            TotalAmount = 0;

            try
            {
                var accounts = await AccountRepoService.AllEnrichedAsync();
                var baseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;
                BaseCulture = baseCurrency.Culture;

                foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Bank))
                {
                    var transactions = (await TransactionRepoService.AllEnrichedForAccountAsync(account.AccountId)).ToList();
                    var balance = await AccountService.CalculateBalance(account, transactions);
                    Data.Add(new EntityModel(account, balance));

                    TotalAmount += await ExchangeRateService.Exchange(account.Currency, baseCurrency, balance);
                }
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        public class EntityModel
        {
            public EntityModel(Account account, decimal balance)
            {
                Name = account.Name;
                Balance = balance;
                Culture = account.Currency.Culture;
            }

            public string Name { get; }

            public decimal Balance { get; }

            public string Culture { get; }
        }
    }
}