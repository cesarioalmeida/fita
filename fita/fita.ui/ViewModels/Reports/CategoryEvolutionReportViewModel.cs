using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Extensions;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using JetBrains.Annotations;
using twentySix.Framework.Core.Extensions;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class CategoryEvolutionReportViewModel : ReportBaseViewModel
    {
        public virtual DateTime FromDate { get; set; } =
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-9);

        public virtual DateTime ToDate { get; set; } = DateTime.Now;

        public virtual Category SelectedCategory { get; set; }

        public LockableCollection<Category> Categories { get; set; } = new();

        public LockableCollection<Model> Data { get; set; } = new();

        public virtual decimal? Average { get; set; }

        public AccountRepoService AccountRepoService { get; set; }

        public CategoryRepoService CategoryRepoService { get; set; }

        public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

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

                if (SelectedCategory is null)
                {
                    return;
                }

                Data.BeginUpdate();
                Data.Clear();

                for (var date = FromDate; date <= ToDate; date = date.AddMonths(1))
                {
                    var startDate = new[] {new DateTime(date.Year, date.Month, 1), FromDate}.Max();
                    var endDate = new[]
                    {
                        new DateTime(startDate.Year, startDate.Month,
                            DateTime.DaysInMonth(startDate.Year, startDate.Month)),
                        ToDate
                    }.Min();

                    var data = await GetModel(startDate, endDate);
                    var month = startDate.ToString("MM-yyyy");
                    Data.Add(new Model(month, data));
                }
            }
            finally
            {
                Data.EndUpdate();

                if (Data?.Any() ?? false)
                {
                    Average = Data?.Average(x => x.Value);
                }
                
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        protected void OnSelectedCategoryChanged() => RefreshData();

        [UsedImplicitly]
        protected void OnFromDateChanged(DateTime oldDate) => RefreshData();

        [UsedImplicitly]
        protected void OnToDateChanged(DateTime oldDate) => RefreshData();

        private async Task<decimal> GetModel(DateTime fromDate, DateTime toDate)
        {
            BaseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;
            var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();
            var transactions =
                (await TransactionRepoService.AllEnrichedBetweenDatesAsync(fromDate, toDate)).ToList();
            var closedPositions = (await ClosedPositionRepoService.AllEnrichedBetweenDatesAsync(fromDate, toDate))
                .ToList();

            var total = 0m;

            if (SelectedCategory.IsCapitalGains() || SelectedCategory.IsCapitalLoses())
            {
                foreach (var position in closedPositions)
                {
                    var account = accounts.Single(x => x.AccountId == position.AccountId);

                    switch (position.ProfitLoss)
                    {
                        case <= 0 when SelectedCategory.IsCapitalLoses():
                            total += await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                                -position.ProfitLoss);
                            break;
                        case > 0 when SelectedCategory.IsCapitalGains():
                            total += await ExchangeRateService.Exchange(account.Currency, BaseCurrency,
                                position.ProfitLoss);
                            break;
                    }
                }

                return total;
            }

            foreach (var transaction in transactions.Where(x => x.Category.CategoryId == SelectedCategory.CategoryId).OrderBy(x => x.Date))
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
            Categories.BeginUpdate();
            Categories.Clear();
            Categories.AddRange((await CategoryRepoService.AllAsync()).Where(x =>
                x.Group is CategoryGroupEnum.PersonalExpenses or CategoryGroupEnum.PersonalIncome));
            Categories.EndUpdate();

            await RefreshData();
        }

        [UsedImplicitly]
        public record Model(string Month, decimal Value);
    }
}