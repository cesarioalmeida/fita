using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Views.Currencies;
using LiteDB;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Currencies
{
    [POCOViewModel]
    public class CurrenciesViewModel : ComposedDocumentViewModelBase
    {
        private bool fireChangeNotification = false;

        public override object Title { get; set; } = "Currencies";

        public FileSettingsService FileSettingsService { get; set; }

        public CurrencyService CurrencyService { get; set; }

        public ExchangeRateService ExchangeRateService { get; set; }

        protected virtual IDocumentManagerService DocumentManagerService =>
            this.GetRequiredService<IDocumentManagerService>();

        public virtual LockableCollection<Currency> Currencies { get; set; } = new();

        public virtual FileSettings FileSettings { get; set; }

        public void Close()
        {
            // notify update of currency if there was a change in the base currency
            if (fireChangeNotification)
            {
            }

            Currencies.Clear();
            DocumentOwner?.Close(this);
        }

        public async Task RefreshData()
        {
            IsBusy = true;

            Currencies.BeginUpdate();
            Currencies.Clear();

            try
            {

                FileSettings = (await FileSettingsService.AllEnrichedAsync())?.FirstOrDefault();

                var currencies = await CurrencyService.AllAsync();
                Currencies.AddRange(currencies);
            }
            finally
            {
                Currencies.EndUpdate();
                IsBusy = false;
            }
        }

        public async Task Edit(Currency currency)
        {
            var viewModel = ViewModelSource.Create<CurrencyDetailsViewModel>();
            viewModel.Currency = currency ?? new Currency {CurrencyId = ObjectId.NewObjectId()};

            var document =
                this.DocumentManagerService.CreateDocument(nameof(CurrencyDetailsView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                if (FileSettings.BaseCurrency == currency)
                {
                    fireChangeNotification = true;
                }

                await RefreshData();
            }
        }

        public async Task Delete(Currency currency)
        {
            if (currency == null)
            {
                return;
            }

            if (WinUIMessageBox.Show(
                $"Are you sure you want to delete the currency {currency.Name}?",
                "Delete currency",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Messenger.Default.Send(await CurrencyService.DeleteAsync(currency.CurrencyId) == Result.Fail
                    ? new NotificationMessage("Failed to delete currency.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Currency {currency.Name} deleted.", NotificationStatusEnum.Success));

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task SetBase(Currency currency)
        {
            if (currency == null)
            {
                return;
            }

            IsBusy = true;

            try
            {
                FileSettings.BaseCurrency = currency;

                Messenger.Default.Send(await FileSettingsService.SaveAsync(FileSettings) == Result.Fail
                    ? new NotificationMessage("Failed to save base currency.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Base currency set to {currency.Name}.", NotificationStatusEnum.Success));

                fireChangeNotification = true;

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Update()
        {
            fireChangeNotification = true;
        }
    }
}