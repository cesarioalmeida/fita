using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Messages;
using JetBrains.Annotations;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Home
{
    [POCOViewModel]
    public class InvestmentsViewModel : ComposedViewModelBase
    {
        public virtual LockableCollection<EntityModel> Data { get; set; } = new();

        public virtual decimal TotalAmount { get; set; }

        public virtual string BaseCulture {get; set;}

        public AccountRepoService AccountRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

        public SecurityPositionRepoService SecurityPositionRepoService { get; set; }

        public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }

        public IExchangeRateService ExchangeRateService { get; set; }

        public IAccountService AccountService { get; set; }

        public InvestmentsViewModel() => Messenger.Default.Register<BaseCurrencyChanged>(this, _ => { RefreshData(); });

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

                foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Investment))
                {
                    var transactions = (await TransactionRepoService.AllEnrichedForAccountAsync(account.AccountId)).ToList();
                    var balance = await AccountService.CalculateBalance(account, transactions);
                    Data.Add(new EntityModel(account.Name, account.Currency.Culture, balance));

                    TotalAmount += await ExchangeRateService.Exchange(account.Currency, baseCurrency, balance);

                    // positions
                    var positions = await SecurityPositionRepoService.AllEnrichedForAccountAsync(account.AccountId);
                    var totalPositions = 0m;

                    foreach (var position in positions)
                    {
                        totalPositions += ((await SecurityHistoryRepoService.FromSecurityEnrichedAsync(position.Security))?
                            .Price?.LatestValue ?? 0m) * position.Quantity;
                    }

                    Data.Add(new EntityModel(account.Name + " - Portfolio", account.Currency.Culture, totalPositions));

                    TotalAmount += await ExchangeRateService.Exchange(account.Currency, baseCurrency, totalPositions);
                }
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public record EntityModel(string Name, string Culture, decimal Balance);
    }
}