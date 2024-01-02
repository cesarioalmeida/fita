using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Messages;
using JetBrains.Annotations;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Home;

[POCOViewModel]
public class HomeViewModel : ComposedViewModelBase, IDisposable
{
    private NetWorth _netWorth;
    
    public virtual BanksViewModel BanksViewModel { get; set; }

    public virtual CreditCardsViewModel CreditCardsViewModel { get; set; }

    public virtual AssetsViewModel AssetsViewModel { get; set; }

    public virtual InvestmentsViewModel InvestmentsViewModel { get; set; }

    public virtual decimal NetWorth => BanksViewModel.TotalAmount + CreditCardsViewModel.TotalAmount +
                                       AssetsViewModel.TotalAmount + InvestmentsViewModel.TotalAmount;

    [Import]
    public TransactionRepoService TransactionRepoService { get; set; }

    [Import]
    public AccountRepoService AccountRepoService { get; set; }

    [Import]
    public FileSettingsRepoService FileSettingsRepoService { get; set; }

    [Import]
    public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

    [Import]
    public NetWorthRepoService NetWorthRepoService { get; set; }

    [Import]
    public IExchangeRateService ExchangeRateService { get; set; }

    public virtual decimal IncomeMonth { get; set; }

    public virtual decimal ExpensesMonth { get; set; }

    public virtual decimal PLMonth { get; set; }

    public HomeViewModel()
    {
        BanksViewModel = ViewModelSource.Create<BanksViewModel>();
        CreditCardsViewModel = ViewModelSource.Create<CreditCardsViewModel>();
        AssetsViewModel = ViewModelSource.Create<AssetsViewModel>();
        InvestmentsViewModel = ViewModelSource.Create<InvestmentsViewModel>();

        BanksViewModel.PropertyChanged += OnChildrenPropertyChanged;
        CreditCardsViewModel.PropertyChanged += OnChildrenPropertyChanged;
        AssetsViewModel.PropertyChanged += OnChildrenPropertyChanged;
        InvestmentsViewModel.PropertyChanged += OnChildrenPropertyChanged;

        RefreshData().ConfigureAwait(false);

        Messenger.Default.Register<BaseCurrencyChanged>(this, _ => { RefreshData().ConfigureAwait(false); });
        Messenger.Default.Register<AccountsChanged>(this, _ => { RefreshData().ConfigureAwait(false); });
        Messenger.Default.Register<SecuritiesChanged>(this, _ => { RefreshData().ConfigureAwait(false); });
    }

    public async Task RefreshData()
    {
        if (IsBusy) return;
        
        IsBusy = true;

        try
        {
            var tasks = new List<Task>();
            
            var baseCurrencyTask = FileSettingsRepoService.GetAll(true);
            tasks.Add(baseCurrencyTask);
            var accountsTask = AccountRepoService.GetAll(true);
            tasks.Add(accountsTask);
            var transactionsTask = TransactionRepoService.GetAllBetweenDates(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            tasks.Add(transactionsTask);
            var closedPositionsTask = ClosedPositionRepoService.GetAllBetweenDates(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            tasks.Add(closedPositionsTask);

            await Task.WhenAll(tasks);
            
            var baseCurrency = baseCurrencyTask.Result.First().BaseCurrency;
            var accounts = accountsTask.Result.ToList();
            var transactions = transactionsTask.Result.ToList();
            var closedPositions = closedPositionsTask.Result.ToList();

            var expenses = 0m;
            var income = 0m;

            foreach (var transaction in transactions)
            {
                var account = accounts.Single(x => x.AccountId == transaction.AccountId);

                switch (transaction.Category.Group)
                {
                    case CategoryGroupEnum.PersonalExpenses:
                        expenses += await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                            transaction.Payment.GetValueOrDefault());
                        break;
                    case CategoryGroupEnum.PersonalIncome:
                        income += await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                            transaction.Deposit.GetValueOrDefault());
                        break;
                }
            }

            foreach (var position in closedPositions)
            {
                var account = accounts.Single(x => x.AccountId == position.AccountId);

                if (position.ProfitLoss <= 0)
                {
                    expenses += await ExchangeRateService.Exchange(account.Currency, baseCurrency, -position.ProfitLoss);
                }
                else
                {
                    income += await ExchangeRateService.Exchange(account.Currency, baseCurrency, position.ProfitLoss);
                }
            }

            ExpensesMonth = expenses;
            IncomeMonth = income;
            PLMonth = IncomeMonth - ExpensesMonth;
        }
        finally
        {
            IsBusy = false;
        }
    }
        
    public void Dispose()
    {
        if (BanksViewModel is not null)
            BanksViewModel.PropertyChanged -= OnChildrenPropertyChanged;

        if (CreditCardsViewModel is not null)
            CreditCardsViewModel.PropertyChanged -= OnChildrenPropertyChanged;

        if (AssetsViewModel is not null)
            AssetsViewModel.PropertyChanged -= OnChildrenPropertyChanged;

        if (InvestmentsViewModel is not null)
            InvestmentsViewModel.PropertyChanged -= OnChildrenPropertyChanged;
    }
        
    private async Task SaveNetWorth()
    {
        _netWorth ??= await NetWorthRepoService.GetForDate(DateTime.Today) ?? new();

        if (_netWorth.Total != NetWorth || _netWorth.Banks != BanksViewModel.TotalAmount ||
            _netWorth.CreditCards != CreditCardsViewModel.TotalAmount ||
            _netWorth.Investments != InvestmentsViewModel.TotalAmount ||
            _netWorth.Assets != AssetsViewModel.TotalAmount)
        {
            _netWorth.Date = DateTime.Today;
            _netWorth.Total = NetWorth;
            _netWorth.Banks = BanksViewModel.TotalAmount;
            _netWorth.CreditCards = CreditCardsViewModel.TotalAmount;
            _netWorth.Investments = InvestmentsViewModel.TotalAmount;
            _netWorth.Assets = AssetsViewModel.TotalAmount;

            await NetWorthRepoService.Save(_netWorth);
        }
    }
        
    private async void OnChildrenPropertyChanged([CanBeNull] object sender, PropertyChangedEventArgs e)
    {
        if (e?.PropertyName?.Equals("TotalAmount") ?? false)
        {
            RaisePropertyChanged(() => NetWorth);
            await SaveNetWorth();
        }
    }
}