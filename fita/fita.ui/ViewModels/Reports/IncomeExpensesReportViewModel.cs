using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.services.Core;
using fita.services.Repositories;
using JetBrains.Annotations;
using twentySix.Framework.Core.Extensions;

namespace fita.ui.ViewModels.Reports;

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

    [Import]
    public AccountRepoService AccountRepoService { get; set; }

    [Import]
    public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

    [Import]
    public TransactionRepoService TransactionRepoService { get; set; }

    [Import]
    public FileSettingsRepoService FileSettingsRepoService { get; set; }

    [Import]
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

            BaseCurrency = (await FileSettingsRepoService.GetAll(true)).First().BaseCurrency;
            var accounts = (await AccountRepoService.GetAll(true)).ToList();
            var transactions =
                (await TransactionRepoService.GetAllBetweenDates(FromDate, ToDate)).ToList();
            var closedPositions = (await ClosedPositionRepoService.GetAllBetweenDates(FromDate, ToDate))
                .ToList();

            TotalIncome = 0m;
            TotalExpenses = 0m;

            foreach (var transaction in transactions.OrderBy(x => x.Date))
            {
                var account = accounts.Single(x => x.AccountId == transaction.AccountId);

                switch (transaction.Category.Group)
                {
                    case CategoryGroupEnum.PersonalExpenses:
                        var payment = await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                            transaction.Payment.GetValueOrDefault());
                        TotalExpenses += payment;
                        Expenses.Add(new(account.Name, transaction.Date, transaction.Category.Name,
                            transaction.Description, payment));
                        break;
                    case CategoryGroupEnum.PersonalIncome:
                        var deposit = await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
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
                    var loss = await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                        -position.ProfitLoss);
                    TotalExpenses += loss;
                    Expenses.Add(
                        new Model(account.Name, position.SellDate, "Capital Loses", "Closed Position", loss));
                }
                else
                {
                    var gain = await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                        position.ProfitLoss);
                    TotalIncome += gain;
                    Income.Add(new Model(account.Name, position.SellDate, "Capital Gains", "Closed Position", gain));
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
    protected void OnFromDateChanged(DateTime oldDate) => RefreshData().ConfigureAwait(false);

    [UsedImplicitly]
    protected void OnToDateChanged(DateTime oldDate) => RefreshData().ConfigureAwait(false);

    [UsedImplicitly]
    public record Model(string Account, DateTime? Date, string Category, string Description, decimal Amount);

    [UsedImplicitly]
    public record PieModel(string Category, decimal Amount);
}