using System;
using System.Collections.Generic;
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
using fita.services.External;
using fita.services.Repositories;
using fita.ui.ViewModels.HistoricalData;
using fita.ui.Views.Currencies;
using fita.ui.Views.HistoricalData;
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
        private bool fireChangeNotification;

        public override object Title { get; set; } = "Currencies";

        public FileSettingsService FileSettingsService { get; set; }

        public CurrencyService CurrencyService { get; set; }

        public ExchangeRateService ExchangeRateService { get; set; }

        public HistoricalDataService HistoricalDataService { get; set; }

        public IExchangeRateDownloadService ExchangeRateDownloadService { get; set; }

        protected virtual IDocumentManagerService DocumentManagerService =>
            this.GetRequiredService<IDocumentManagerService>();

        public virtual LockableCollection<CurrenciesModel> Data { get; set; } = new();

        public virtual FileSettings FileSettings { get; set; }

        public void Close()
        {
            // notify update of currency if there was a change in the base currency
            if (fireChangeNotification)
            {
            }

            Data.Clear();
            DocumentOwner?.Close(this);
        }

        public async Task RefreshData()
        {
            IsBusy = true;

            Data.BeginUpdate();
            Data.Clear();

            try
            {
                FileSettings = (await FileSettingsService.AllEnrichedAsync())?.FirstOrDefault();

                var currencies = await CurrencyService.AllAsync();
                var exchangeRates = await ExchangeRateService.AllFromCurrencyEnrichedAsync(FileSettings?.BaseCurrency);

                var data = currencies.Select(c =>
                    new CurrenciesModel(c, exchangeRates.FirstOrDefault(x => x.ToCurrency.CurrencyId == c.CurrencyId)));

                Data.AddRange(data);
            }
            finally
            {
                Data.EndUpdate();
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
                fireChangeNotification = true;

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
                var exchangeRatesToDelete = await ExchangeRateService.AllWithCurrencyEnrichedAsync(currency);

                foreach (var rate in exchangeRatesToDelete)
                {
                    await HistoricalDataService.DeleteAsync(rate.Rate.HistoricalDataId);
                    await ExchangeRateService.DeleteAsync(rate.ExchangeRateId);
                }

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

        public bool CanDelete(Currency currency)
        {
            return currency?.CurrencyId != FileSettings.BaseCurrency.CurrencyId;
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
                    : new NotificationMessage($"Base currency set to {currency.Name}.",
                        NotificationStatusEnum.Success));

                fireChangeNotification = true;

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool CanSetBase(Currency currency)
        {
            return currency?.CurrencyId != FileSettings.BaseCurrency.CurrencyId;
        }

        public async Task Update()
        {
            IsBusy = true;

            try
            {
                fireChangeNotification = true;

                var currentExchangeRates =
                    (await ExchangeRateService.AllFromCurrencyEnrichedAsync(FileSettings.BaseCurrency)).ToList();

                foreach (var currency in Data.Select(x => x.Currency)
                    .Where(x => x.CurrencyId != FileSettings.BaseCurrency.CurrencyId))
                {
                    var exchangeRate =
                        currentExchangeRates.SingleOrDefault(x => x.ToCurrency.CurrencyId == currency.CurrencyId) ??
                        new ExchangeRate
                        {
                            ExchangeRateId = ObjectId.NewObjectId(),
                            FromCurrency = FileSettings.BaseCurrency, ToCurrency = currency,
                            Rate = new data.Models.HistoricalData
                            {
                                HistoricalDataId = ObjectId.NewObjectId(),
                                Name = $"Exchange rate {FileSettings.BaseCurrency.Name} => {currency.Name}"
                            }
                        };

                    Messenger.Default.Send(new NotificationMessage($"Updating currency {currency.Name}..."));

                    if (await ExchangeRateDownloadService.UpdateAsync(exchangeRate) == Result.Fail)
                    {
                        Messenger.Default.Send(new NotificationMessage($"Could not update currency {currency.Name}",
                            NotificationStatusEnum.Error));
                    }
                }

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task History(CurrenciesModel model)
        {
            var viewModel = ViewModelSource.Create<HistoricalDataViewModel>();
            viewModel.Model = model.ExchangeRate.Rate;

            var document = this.DocumentManagerService.CreateDocument(nameof(HistoricalDataView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                fireChangeNotification = true;

                await RefreshData();
            }
        }

        public bool CanHistory(CurrenciesModel model)
        {
            return model?.Currency.CurrencyId != FileSettings.BaseCurrency.CurrencyId;
        }

        public class CurrenciesModel
        {
            public CurrenciesModel(Currency currency, ExchangeRate exchangeRate)
            {
                Currency = currency;
                ExchangeRate = exchangeRate;
            }

            public Currency Currency { get; }

            public ExchangeRate ExchangeRate { get; }

            public DateTime? LatestDate => ExchangeRate?.Rate.LatestDate;

            public decimal? LatestValue => ExchangeRate?.Rate.LatestValue;

            public IEnumerable<decimal> History => ExchangeRate?.Rate.Data.Values.Select(x => x.Value);
        }
    }
}