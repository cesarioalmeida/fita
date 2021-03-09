using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class NetWorthReportViewModel : ComposedViewModelBase
    {
        public LockableCollection<Model> Data { get; set; } = new();

        public virtual Currency BaseCurrency { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

        public AccountRepoService AccountRepoService { get; set; }

        public TransactionRepoService TransactionRepoService { get; set; }

        public SecurityPositionRepoService SecurityPositionRepoService { get; set; }

        public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }

        public IExchangeRateService ExchangeRateService { get; set; }

        public IAccountService AccountService { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;
            Data.BeginUpdate();

            try
            {
                Data.Clear();

                var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();
                BaseCurrency = (await FileSettingsRepoService.AllEnrichedAsync()).First().BaseCurrency;

                var allTransactions = (await TransactionRepoService.AllEnrichedAsync()).ToList();

                for (var date = DateTime.Now.AddYears(-1); date <= DateTime.Now; date = date.AddDays(1))
                {
                    var banks = 0m;
                    foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Bank))
                    {
                        var transactions = allTransactions
                            .Where(x => x.Date <= date && x.AccountId == account.AccountId).ToList();
                        var balance = await AccountService.CalculateBalance(account, transactions);
                        banks += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);
                    }

                    var creditCards = 0m;
                    foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.CreditCard))
                    {
                        var transactions = allTransactions
                            .Where(x => x.Date <= date && x.AccountId == account.AccountId).ToList();
                        var balance = await AccountService.CalculateBalance(account, transactions);
                        creditCards += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);
                    }

                    var assets = 0m;
                    foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Asset))
                    {
                        var transactions = allTransactions
                            .Where(x => x.Date <= date && x.AccountId == account.AccountId).ToList();
                        var balance = await AccountService.CalculateBalance(account, transactions);
                        assets += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);
                    }

                    var investments = 0m;
                    foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Investment))
                    {
                        var transactions = allTransactions
                            .Where(x => x.Date <= date && x.AccountId == account.AccountId).ToList();
                        var balance = await AccountService.CalculateBalance(account, transactions);
                        investments += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, balance);

                        // positions
                        var positions = await SecurityPositionRepoService.AllEnrichedForAccountAsync(account.AccountId);
                        var totalPositions = 0m;
                        foreach (var position in positions)
                        {
                            totalPositions += ((await SecurityHistoryRepoService.FromSecurityEnrichedAsync(position.Security))?
                                .Price?.LatestValue ?? 0m) * position.Quantity;
                        }

                        investments += await ExchangeRateService.Exchange(account.Currency, BaseCurrency, totalPositions);
                    }


                    Data.Add(new Model(date, banks + creditCards + assets + investments, banks, investments, creditCards, assets));
                }
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        public class Model
        {
            public Model(DateTime date, decimal netWorth, decimal banks, decimal investment, decimal creditCards,
                decimal assets)
            {
                Date = date;
                NetWorth = netWorth;
                Banks = banks;
                Investments = investment;
                CreditCards = creditCards;
                Assets = assets;
            }

            public DateTime Date { get; }

            public decimal NetWorth { get; }

            public decimal Banks { get; }

            public decimal Investments { get; }

            public decimal CreditCards { get; }

            public decimal Assets { get; }
        }
    }
}