using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Messages;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Home;

[POCOViewModel]
public class AssetsViewModel : ComposedViewModelBase
{
    public virtual LockableCollection<EntityModel> Data { get; set; } = new();

    public virtual decimal TotalAmount { get; set; }

    public virtual string BaseCulture {get; set;}

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

    public AssetsViewModel() 
        => Messenger.Default.Register<BaseCurrencyChanged>(this, _ => { RefreshData().ConfigureAwait(false); });

    public async Task RefreshData()
    {
        IsBusy = true;

        Data.BeginUpdate();
        Data.Clear();

        TotalAmount = 0;

        try
        {
            var accounts = await AccountRepoService.GetAll(true);
            var baseCurrency = (await FileSettingsRepoService.GetAll(true)).First().BaseCurrency;
            BaseCulture = baseCurrency.Culture;

            foreach (var account in accounts.Where(x => x.Type == AccountTypeEnum.Asset))
            {
                var transactions = (await TransactionRepoService.GetAllForAccount(account.AccountId)).ToList();
                var balance = await AccountService.CalculateBalance(account, transactions);
                Data.Add(new EntityModel(account, balance));

                TotalAmount += await ExchangeRateService.Exchange(account.Currency, baseCurrency, balance);
            }
        }
        finally
        {
            Data.EndUpdate();
            IsBusy = false;
        }
    }

    public record EntityModel
    {
        public EntityModel(Account account, decimal balance)
        {
            Name = account.Name;
            Balance = balance;
            Culture = account.Currency.Culture;
        }

        public string Name { get; }

        public decimal Balance { get; }

        public string Culture { get; }
    }
}