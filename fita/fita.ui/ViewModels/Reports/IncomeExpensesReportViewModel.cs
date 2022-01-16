using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.services.Core;
using fita.services.Repositories;
using JetBrains.Annotations;
using twentySix.Framework.Core.Extensions;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class IncomeExpensesReportViewModel : ReportBaseViewModel
    {
        public virtual DateTime FromDate { get; set; } = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public virtual DateTime ToDate { get; set; } = DateTime.Now;

        public LockableCollection<Model> Income { get; set; } = new();

        public LockableCollection<Model> Expenses { get; set; } = new();

        public LockableCollection<PieModel> IncomePie { get; set; } = new();

        public LockableCollection<PieModel> ExpensesPie { get; set; } = new();

        public decimal? TotalIncome { get; set; }

        public decimal? TotalExpenses { get; set; }

        public AccountRepoService AccountRepoService { get; set; }

        public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

        public IExchangeRateService ExchangeRateService { get; set; }
        

        public override async Task RefreshData()
        {
            IsBusy = true;
            Income.BeginUpdate();
            Expenses.BeginUpdate();
            IncomePie.BeginUpdate();
            ExpensesPie.BeginUpdate();

            try
            {
                Income.Clear();
                Expenses.Clear();

                var baseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;
                var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();
                var transactions =
                    (await TransactionRepoService.AllEnrichedBetweenDatesAsync(FromDate, ToDate)).ToList();
                var closedPositions = (await ClosedPositionRepoService.AllEnrichedBetweenDatesAsync(FromDate, ToDate))
                    .ToList();

                TotalIncome = 0m;
                TotalExpenses = 0m;

                foreach (var transaction in transactions.OrderBy(x => x.Date))
                {
                    var account = accounts.Single(x => x.AccountId == transaction.AccountId);

                    switch (transaction.Category.Group)
                    {
                        case CategoryGroupEnum.PersonalExpenses:
                            var payment = await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                                transaction.Payment.GetValueOrDefault());
                            TotalExpenses += payment;
                            Expenses.Add(new(account.Name, transaction.Date, transaction.Category.Name,
                                transaction.Description, payment));
                            break;
                        case CategoryGroupEnum.PersonalIncome:
                            var deposit = await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                                transaction.Deposit.GetValueOrDefault());
                            TotalIncome += deposit;
                            Income.Add(new(account.Name, transaction.Date, transaction.Category.Name,
                                transaction.Description, deposit));
                            break;
                    }
                }

                foreach (var position in closedPositions)
                {
                    var account = accounts.Single(x => x.AccountId == position.AccountId);

                    if (position.ProfitLoss <= 0)
                    {
                        var loss = await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                            -position.ProfitLoss);
                        TotalExpenses += loss;
                        Expenses.Add(
                            new Model(account.Name, position.SellDate, "Capital Loss", "Closed Position", loss));
                    }
                    else
                    {
                        var gain = await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                            position.ProfitLoss);
                        TotalIncome += gain;
                        Income.Add(new Model(account.Name, position.SellDate, "Capital Gain", "Closed Position", gain));
                    }
                }

                // pie charts
                IncomePie.Clear();
                ExpensesPie.Clear();

                IncomePie.AddRange(Income.GroupBy(x => x.Category)
                    .Select(x => new PieModel(x.Key, x.Sum(_ => _.Amount))));
                ExpensesPie.AddRange(Expenses.GroupBy(x => x.Category)
                    .Select(x => new PieModel(x.Key, x.Sum(_ => _.Amount))));
            }
            finally
            {
                Income.EndUpdate();
                Expenses.EndUpdate();
                IncomePie.EndUpdate();
                ExpensesPie.EndUpdate();
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        protected void OnFromDateChanged(DateTime oldDate) => RefreshData();

        [UsedImplicitly]
        protected void OnToDateChanged(DateTime oldDate) => RefreshData();

        [UsedImplicitly]
        public record Model(string Account, DateTime? Date, string Category, string Description, decimal Amount);

        [UsedImplicitly]
        public record PieModel(string Category, decimal Amount);
    }
}