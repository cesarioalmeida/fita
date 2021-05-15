using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.services.Core;
using fita.services.Repositories;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class PLMonthReportViewModel : ReportBaseViewModel
    {
        public virtual DateTime FromDate { get; set; } =
            new(DateTime.Now.AddMonths(-6).Year, DateTime.Now.AddMonths(-6).Month, 1);

        public virtual DateTime ToDate { get; set; } = DateTime.Now;
        
        public LockableCollection<Model> IncomeData { get; set; } = new();
        
        public LockableCollection<Model> ExpensesData { get; set; } = new();
        
        public LockableCollection<Model> PLData { get; set; } = new();

        public AccountRepoService AccountRepoService { get; set; }

        public ClosedPositionRepoService ClosedPositionRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

        public IExchangeRateService ExchangeRateService { get; set; }


        public override async Task RefreshData()
        {
            IsBusy = true;

            try
            {
                IncomeData.Clear();
                ExpensesData.Clear();
                PLData.Clear();

                if (FromDate > ToDate)
                {
                    return;
                }
                
                IncomeData.BeginUpdate();
                ExpensesData.BeginUpdate();
                PLData.BeginUpdate();

                for (DateTime date = FromDate; date <= ToDate; date = date.AddMonths(1))
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
                    IncomeData.Add(new Model(month, data.Item1));
                    ExpensesData.Add(new Model(month, -data.Item2));
                    PLData.Add(new Model(month, data.Item1-data.Item2));
                }
            }
            finally
            {
                IncomeData.EndUpdate();
                ExpensesData.EndUpdate();
                PLData.EndUpdate();
                
                IsBusy = false;
            }
        }

        protected void OnFromDateChanged(DateTime oldDate)
        {
            RefreshData();
        }

        protected void OnToDateChanged(DateTime oldDate)
        {
            RefreshData();
        }

        private async Task<Tuple<decimal, decimal>> GetModel(DateTime fromDate, DateTime toDate)
        {
            var baseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;
            var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();
            var transactions =
                (await TransactionRepoService.AllEnrichedBetweenDatesAsync(fromDate, toDate)).ToList();
            var closedPositions = (await ClosedPositionRepoService.AllEnrichedBetweenDatesAsync(fromDate, toDate))
                .ToList();

            var totalIncome = 0m;
            var totalExpenses = 0m;

            foreach (var transaction in transactions.OrderBy(x => x.Date))
            {
                var account = accounts.Single(x => x.AccountId == transaction.AccountId);

                switch (transaction.Category.Group)
                {
                    case CategoryGroupEnum.PersonalExpenses:
                    case CategoryGroupEnum.BusinessExpenses:
                        var payment = await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                            transaction.Payment.GetValueOrDefault());
                        totalExpenses += payment;
                        break;
                    case CategoryGroupEnum.PersonalIncome:
                    case CategoryGroupEnum.BusinessIncome:
                        var deposit = await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                            transaction.Deposit.GetValueOrDefault());
                        totalIncome += deposit;
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
                    totalExpenses += loss;
                }
                else
                {
                    var gain = await ExchangeRateService.Exchange(account.Currency, baseCurrency,
                        position.ProfitLoss);
                    totalIncome += gain;
                }
            }

            return Tuple.Create(totalIncome, totalExpenses);
        }

        public class Model
        {
            public Model(string month, decimal value)
            {
                Month = month;
                Value = value;
            }

            public string Month { get; }
            
            public decimal Value { get; }
        }
    }
}