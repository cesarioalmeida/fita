using DevExpress.Mvvm.DataAnnotations;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels
{
    [POCOViewModel]
    public class CurrenciesViewModel : ComposedDocumentViewModelBase
    {
        private bool fireChangeNotification = false;

        public override object Title { get; set; } = "Currencies";

        public FileSettingsService FileSettingsService { get; set; }

        public CurrencyService CurrencyService { get; set; }

        public ExchangeRateService ExchangeRateService { get; set; }

        protected virtual IDocumentManagerService DocumentManagerService => this.GetRequiredService<IDocumentManagerService>();

        public ObservableCollection<Currency> Currencies { get; set; }

        public FileSettings FileSettings { get; set; }

        public void Close()
        {
            // notify update of currency if there was a change in the base currency
            if(fireChangeNotification)
            {

            }

            Currencies.Clear();
            DocumentOwner?.Close(this);
        }
        
        public void DeleteRow()
        {
            // if (WinUIMessageBox.Show(
            //     $"Are you sure you want to delete the date\n{SelectedRow.Date.ToShortDateString()}?",
            //     "delete data",
            //     MessageBoxButton.OKCancel,
            //     MessageBoxImage.Question) != MessageBoxResult.OK)
            // {
            //     return;
            // }
            //
            // SelectedCurrency.ExchangeData.Delete(SelectedRow);
        }

        // public bool CanDeleteRow()
        // {
        //     return SelectedRow != null;
        // }

        public async Task RefreshData()
        {
            IsBusy = true;

            Currencies = new ObservableCollection<Currency>();

            try
            {
                FileSettings = (await FileSettingsService.AllEnrichedAsync())?.FirstOrDefault();

                var currencies = await CurrencyService.AllAsync();
                Currencies.AddRange(currencies);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Add()
        {
            ;
        }

        public async Task SetBaseCurrency(Currency currency)
        {
            IsBusy = true;

            fireChangeNotification = true;

            try
            {
                FileSettings.BaseCurrency = currency;
                if (await FileSettingsService.SaveAsync(FileSettings) == Result.Fail)
                {
                    Messenger.Default.Send(new NotificationMessage("Failed to save base currency.",
                        NotificationStatusEnum.Error));
                }
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