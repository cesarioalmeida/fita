using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;

namespace fita.ui.ViewModels.Reports;

[POCOViewModel]
public class NetWorthReportViewModel : ReportBaseViewModel
{
    private const int StepDays = 15;
    private readonly DateTime _fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddYears(-1);

    public LockableCollection<Model> Data { get; set; } = new();

    [Import]
    public AccountRepoService AccountRepoService { get; set; }
        
    [Import]
    public TransactionRepoService TransactionRepoService { get; set; }
        
    [Import]
    public FileSettingsRepoService FileSettingsRepoService { get; set; }
        
    [Import]
    public IExchangeRateService ExchangeRateService { get; set; }
        
    [Import]
    public IAccountService AccountService { get; set; }
        
    [Import]
    public IInvestmentPricingService InvestmentPricingService { get; set; }
        
    [Import]
    public TradeRepoService TradeRepoService { get; set; }
        
    public override async Task RefreshData()
    {
        IsBusy = true;
        Data.BeginUpdate();
        Data.Clear();
            
        try
        {
            for (var date = _fromDate; date <= DateTime.Now; date = date.AddDays(StepDays))
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
        BaseCurrency = (await FileSettingsRepoService.GetAll(true)).First().BaseCurrency;
        var accounts = (await AccountRepoService.GetAll(true)).ToList();
            
        // banks
        foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Bank))
        {
            var transactions = (await TransactionRepoService.GetAllToDateForAccount(date, account.AccountId)).ToList();
            var balance = await AccountService.CalculateBalance(account, transactions);
            model.Banks += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);
        }

        // credit cards
        foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.CreditCard))
        {
            var transactions = (await TransactionRepoService.GetAllToDateForAccount(date, account.AccountId)).ToList();
            var balance = await AccountService.CalculateBalance(account, transactions);
            model.CreditCards += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);
        }
            
        // assets
        foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Asset))
        {
            var transactions = (await TransactionRepoService.GetAllToDateForAccount(date, account.AccountId)).ToList();
            var balance = await AccountService.CalculateBalance(account, transactions);
            model.Assets += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);
        }
            
        // investments
        foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Investment))
        {
            var transactions = (await TransactionRepoService.GetAllToDateForAccount(date, account.AccountId)).ToList();
            var balance = await AccountService.CalculateBalance(account, transactions);
            model.Investments += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);
                
            var trades = (await TradeRepoService.GetAllToDateForAccount(date, account.AccountId)).ToList();
            var positions = trades.GroupBy(x => x.Security)
                .ToDictionary(x => x.Key, x => x.Sum(_ => _.Quantity * (_.Action == TradeActionEnum.Buy ? 1m : -1m)));

            var totalPositions = 0m;
            foreach (var position in positions)
            {
                totalPositions += await InvestmentPricingService.GetPrice(position.Key, date) * position.Value;
            }
                
            model.Investments += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, totalPositions);
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