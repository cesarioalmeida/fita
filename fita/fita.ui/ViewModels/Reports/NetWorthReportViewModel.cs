using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class NetWorthReportViewModel : ReportBaseViewModel
    {
        private DateTime _fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddYears(-1);
        private int _stepDays = 15;
        
        public LockableCollection<Model> Data { get; set; } = new();

        public virtual Currency BaseCurrency { get; set; }
        
        public AccountRepoService AccountRepoService { get; set; }
        
        public TransactionRepoService TransactionRepoService { get; set; }
        
        public FileSettingsRepoService FileSettingsRepoService { get; set; }
        
        public IExchangeRateService ExchangeRateService { get; set; }
        
        public IAccountService AccountService { get; set; }
        
        public SecurityPositionRepoService SecurityPositionRepoService { get; set; }

        public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }
        
        public ClosedPositionRepoService ClosedPositionRepoService { get; set; }
        
        public override async Task RefreshData()
        {
            IsBusy = true;
            Data.BeginUpdate();
            Data.Clear();
            
            try
            {
                for (var date = _fromDate; date <= DateTime.Now; date = date.AddDays(_stepDays))
                {
                    var model = await GetModel(date);
                    Data.Add(new Model(model));
                }
                
                // guarantee that the last point is always now
                Data.Add(new Model(await GetModel(DateTime.Now)));
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        private async Task<NetWorth> GetModel(DateTime date)
        {
            var model = new NetWorth {Date = date};
            var baseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;
            var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();
            
            // banks
            foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Bank))
            {
                var transactions = (await TransactionRepoService.AllEnrichedToDateForAccountAsync(date, account.AccountId)).ToList();
                var balance = await AccountService.CalculateBalance(account, transactions);
                model.Banks += await ExchangeRateService.Exchange(account.Currency, baseCurrency, balance);
            }

            // credit cards
            foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.CreditCard))
            {
                var transactions = (await TransactionRepoService.AllEnrichedToDateForAccountAsync(date, account.AccountId)).ToList();
                var balance = await AccountService.CalculateBalance(account, transactions);
                model.CreditCards += await ExchangeRateService.Exchange(account.Currency, baseCurrency, balance);
            }
            
            // assets
            foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Asset))
            {
                var transactions = (await TransactionRepoService.AllEnrichedToDateForAccountAsync(date, account.AccountId)).ToList();
                var balance = await AccountService.CalculateBalance(account, transactions);
                model.Assets += await ExchangeRateService.Exchange(account.Currency, baseCurrency, balance);
            }
            
            // investments
            foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Investment))
            {
                var transactions = (await TransactionRepoService.AllEnrichedToDateForAccountAsync(date, account.AccountId)).ToList();
                var balance = await AccountService.CalculateBalance(account, transactions);
                model.Investments += await ExchangeRateService.Exchange(account.Currency, baseCurrency, balance);
                
                // at the moment we have no way of determining the state of the portfolio at a given point in time,
                // so we are just adding today's position's value
                var positions = await SecurityPositionRepoService.AllEnrichedForAccountAsync(account.AccountId);
                
                var totalPositions = 0m;
                
                foreach (var position in positions)
                {
                    totalPositions += ((await SecurityHistoryRepoService.FromSecurityEnrichedAsync(position.Security))?
                        .Price?.LatestValue ?? 0m) * position.Quantity;
                }

                model.Investments += await ExchangeRateService.Exchange(account.Currency, baseCurrency, totalPositions);
            }
            
            return model;
        }

        public record Model
        {
            public Model(NetWorth item)
            {
                Date = item.Date;
                Banks = item.Banks;
                Investments = item.Investments;
                CreditCards = item.CreditCards;
                Assets = item.Assets;
            }

            public DateTime Date { get; }

            public decimal NetWorth => Banks + CreditCards + Assets + Investments;

            public decimal Banks { get; }

            public decimal Investments { get; }

            public decimal CreditCards { get; }

            public decimal Assets { get; }
        }
    }
}