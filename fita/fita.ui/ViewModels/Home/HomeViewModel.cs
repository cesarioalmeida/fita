using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using fita.data.Enums;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Messages;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Home
{
    [POCOViewModel]
    public class HomeViewModel : ComposedViewModelBase
    {
        public virtual BanksViewModel BanksViewModel { get; set; }

        public virtual CreditCardsViewModel CreditCardsViewModel { get; set; }

        public virtual AssetsViewModel AssetsViewModel { get; set; }

        public virtual InvestmentsViewModel InvestmentsViewModel { get; set; }

        public virtual decimal NetWorth => BanksViewModel.TotalAmount + CreditCardsViewModel.TotalAmount +
                                           AssetsViewModel.TotalAmount + InvestmentsViewModel.TotalAmount;

        public TransactionRepoService TransactionRepoService { get; set; }

        public AccountRepoService AccountRepoService { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

        public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

        public IExchangeRateService ExchangeRateService { get; set; }

        public virtual decimal IncomeMonth { get; set; }

        public virtual decimal ExpensesMonth { get; set; }

        public HomeViewModel()
        {
            BanksViewModel = ViewModelSource.Create<BanksViewModel>();
            CreditCardsViewModel = ViewModelSource.Create<CreditCardsViewModel>();
            AssetsViewModel = ViewModelSource.Create<AssetsViewModel>();
            InvestmentsViewModel = ViewModelSource.Create<InvestmentsViewModel>();

            RefreshData();
            
            Messenger.Default.Register<BaseCurrencyChanged>(this, _ => { RefreshData(); });
        }


        public async Task RefreshData()
        {
            IsBusy = true;

            try
            {
                var baseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;

                var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();

                var transactions =
                    (await TransactionRepoService.AllEnrichedBetweenDatesAsync(new DateTime(DateTime.Now.Year,
                        DateTime.Now.Month, 1))).ToList();

                var closedPositions = (await ClosedPositionRepoService.AllEnrichedBetweenDatesAsync(new DateTime(DateTime.Now.Year,
                    DateTime.Now.Month, 1))).ToList();

                var expenses = 0m;
                var income = 0m;

                foreach (var transaction in transactions)
                {
                    var account = accounts.Single(x => x.AccountId == transaction.AccountId);

                    switch (transaction.Category.Group)
                    {
                        case CategoryGroupEnum.PersonalExpenses:
                        case CategoryGroupEnum.BusinessExpenses:
                            expenses += await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                                transaction.Payment.GetValueOrDefault());
                            expenses += closedPositions.Where(x => x.ProfitLoss <= 0).Sum(x => x.ProfitLoss);
                            break;
                        case CategoryGroupEnum.PersonalIncome:
                        case CategoryGroupEnum.BusinessIncome:
                            income += await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                                transaction.Deposit.GetValueOrDefault());
                            income += closedPositions.Where(x => x.ProfitLoss > 0).Sum(x => x.ProfitLoss);
                            break;
                    }
                }

                ExpensesMonth = expenses;
                IncomeMonth = income;
            }
            finally
            {
                IsBusy = false;
                RaisePropertyChanged(() => NetWorth);
            }
        }
    }
}