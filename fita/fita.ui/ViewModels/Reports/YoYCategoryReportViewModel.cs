using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using fita.data.Enums;
using fita.data.Extensions;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using JetBrains.Annotations;

namespace fita.ui.ViewModels.Reports;

[POCOViewModel]
public class YoYCategoryReportViewModel : ReportBaseViewModel
{
    private readonly List<Category> _categories = new();
        
    public virtual DateTime FromDate { get; set; } = new(DateTime.Now.Year, 1, 1);

    public virtual DateTime ToDate { get; set; } = DateTime.Now;

    public ObservableCollection<Model> Data { get; set; } = new();

    [Import]
    public AccountRepoService AccountRepoService { get; set; }

    [Import]
    public CategoryRepoService CategoryRepoService { get; set; }

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

        try
        {
            if (FromDate > ToDate)
            {
                return;
            }

            Data.Clear();

            foreach (var category in _categories)
            {
                var chart = new List<Tuple<string, decimal>>();
                    
                for (var nYear = -2; nYear <= 0; nYear++)
                {
                    var startDate = FromDate.AddYears(nYear);
                    var endDate = ToDate.AddYears(nYear);
                        
                    var value = await GetAmount(category, startDate, endDate);
                        
                    chart.Add(Tuple.Create($"{nYear} yr", value));
                }

                Data.Add(new Model(category.Name, chart));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    protected void OnFromDateChanged(DateTime oldDate) => RefreshData().ConfigureAwait(false);

    [UsedImplicitly]
    protected void OnToDateChanged(DateTime oldDate) => RefreshData().ConfigureAwait(false);

    private async Task<decimal> GetAmount(Category category, DateTime fromDate, DateTime toDate)
    {
        BaseCurrency = (await FileSettingsRepoService.GetAll(true)).First().BaseCurrency;
        var accounts = (await AccountRepoService.GetAll(true)).ToList();
        var transactions =
            (await TransactionRepoService.GetAllBetweenDates(fromDate, toDate)).ToList();
        var closedPositions = (await ClosedPositionRepoService.GetAllBetweenDates(fromDate, toDate))
            .ToList();

        var total = 0m;

        if (category.IsCapitalGains() || category.IsCapitalLoses())
        {
            foreach (var position in closedPositions)
            {
                var account = accounts.Single(x => x.AccountId == position.AccountId);

                switch (position.ProfitLoss)
                {
                    case <= 0 when category.IsCapitalLoses():
                        total += await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                            -position.ProfitLoss);
                        break;
                    case > 0 when category.IsCapitalGains():
                        total += await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                            position.ProfitLoss);
                        break;
                }
            }

            return total;
        }

        foreach (var transaction in transactions.Where(x => x.Category.CategoryId == category.CategoryId).OrderBy(x => x.Date))
        {
            var account = accounts.Single(x => x.AccountId == transaction.AccountId);

            total += await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                transaction.Payment.GetValueOrDefault() + transaction.Deposit.GetValueOrDefault());
        }

        return total;
    }

    [UsedImplicitly]
    public async Task OnViewLoaded()
    {
        _categories.Clear();
        _categories.AddRange((await CategoryRepoService.GetAll()).Where(x =>
            x.Group is CategoryGroupEnum.PersonalExpenses or CategoryGroupEnum.PersonalIncome));

        await RefreshData();
    }

    [UsedImplicitly]
    public record Model(string Category, List<Tuple<string, decimal>> ChartData);
}