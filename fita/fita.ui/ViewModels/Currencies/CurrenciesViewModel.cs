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
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Messages;
using fita.ui.ViewModels.HistoricalData;
using fita.ui.Views.Currencies;
using fita.ui.Views.HistoricalData;
using JetBrains.Annotations;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Currencies
{
    [POCOViewModel]
    public class CurrenciesViewModel : ComposedDocumentViewModelBase
    {
        private bool _fireChangeNotification;

        public LoggingService LoggingService { get; set; }

        public FileSettingsRepoService FileSettingsRepoService { get; set; }

        public CurrencyRepoService CurrencyRepoService { get; set; }

        public ExchangeRateRepoService ExchangeRateRepoService { get; set; }

        public HistoricalDataRepoService HistoricalDataRepoService { get; set; }

        public IExchangeRateService ExchangeRateService { get; set; }

        protected IDocumentManagerService DocumentManagerService =>
            this.GetRequiredService<IDocumentManagerService>("ModalWindowDocumentService");

        public virtual LockableCollection<CurrenciesModel> Data { get; set; } = new();

        public virtual FileSettings FileSettings { get; set; }

        [UsedImplicitly]
        public void Close()
        {
            if (_fireChangeNotification)
            {
                Messenger.Default.Send(new BaseCurrencyChanged());
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
                FileSettings = (await FileSettingsRepoService.AllEnrichedAsync())?.FirstOrDefault();

                var currencies = await CurrencyRepoService.AllAsync();

                // if there's no base currency and there's at least one currency => set base currency
                if (FileSettings?.BaseCurrency == null)
                {
                    if (currencies.Any())
                    {
                        await SetBase(currencies.First(), false);
                    }
                    else
                    {
                        return;
                    }
                }

                var exchangeRates =
                    await ExchangeRateRepoService.AllFromCurrencyEnrichedAsync(FileSettings?.BaseCurrency);

                var data = currencies.Select(c =>
                    new CurrenciesModel(FileSettings.BaseCurrency, c,
                        exchangeRates.FirstOrDefault(x => x.ToCurrency.CurrencyId == c.CurrencyId)));

                Data.AddRange(data);
            }
            catch (Exception ex)
            {
                LoggingService.Error($"@CurrenciesViewModel.RefreshData(): {ex.Message}");
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;

                if (_fireChangeNotification)
                {
                    Messenger.Default.Send(new BaseCurrencyChanged());
                }
            }
        }

        [UsedImplicitly]
        public async Task Edit(Currency currency)
        {
            var viewModel = ViewModelSource.Create<CurrencyDetailsViewModel>();
            viewModel.Currency = currency ?? new Currency();

            var document = DocumentManagerService.CreateDocument(nameof(CurrencyDetailsView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                _fireChangeNotification = true;

                await RefreshData();
            }
        }

        [UsedImplicitly]
        public async Task Delete(Currency currency)
        {
            if (currency is null)
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
                var exchangeRatesToDelete = await ExchangeRateRepoService.AllWithCurrencyEnrichedAsync(currency);

                if (exchangeRatesToDelete is not null)
                {
                    foreach (var rate in exchangeRatesToDelete)
                    {
                        await HistoricalDataRepoService.DeleteAsync(rate.Rate.HistoricalDataId);
                        await ExchangeRateRepoService.DeleteAsync(rate.ExchangeRateId);
                    }
                }

                Messenger.Default.Send(await CurrencyRepoService.DeleteAsync(currency.CurrencyId) == Result.Fail
                    ? new NotificationMessage("Failed to delete currency.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Currency {currency.Name} deleted.", NotificationStatusEnum.Success));

                await RefreshData();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"{nameof(Delete)}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public bool CanDelete(Currency currency) => currency?.CurrencyId != FileSettings.BaseCurrency.CurrencyId;

        public async Task SetBase(Currency currency)
        {
            if (currency is null)
            {
                return;
            }

            IsBusy = true;

            try
            {
                FileSettings.BaseCurrency = currency;

                Messenger.Default.Send(await FileSettingsRepoService.SaveAsync(FileSettings) == Result.Fail
                    ? new NotificationMessage("Failed to save base currency.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Base currency set to {currency.Name}.",
                        NotificationStatusEnum.Success));

                _fireChangeNotification = true;

                await RefreshData();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"{nameof(SetBase)}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SetBase(Currency currency, bool refresh = true)
        {
            if (currency is null)
            {
                return;
            }

            IsBusy = true;

            try
            {
                FileSettings.BaseCurrency = currency;

                Messenger.Default.Send(await FileSettingsRepoService.SaveAsync(FileSettings) == Result.Fail
                    ? new NotificationMessage("Failed to save base currency.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Base currency set to {currency.Name}.",
                        NotificationStatusEnum.Success));

                _fireChangeNotification = true;

                if (refresh)
                {
                    await RefreshData();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error($"{nameof(SetBase)}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public bool CanSetBase(Currency currency) => currency?.CurrencyId != FileSettings.BaseCurrency.CurrencyId;

        public async Task Update()
        {
            IsBusy = true;

            try
            {
                _fireChangeNotification = true;

                var currentExchangeRates =
                    (await ExchangeRateRepoService.AllFromCurrencyEnrichedAsync(FileSettings.BaseCurrency)).ToList();

                foreach (var currency in Data.Select(x => x.Currency)
                             .Where(x => x.CurrencyId != FileSettings.BaseCurrency.CurrencyId))
                {
                    var exchangeRate =
                        currentExchangeRates.SingleOrDefault(x => x.ToCurrency.CurrencyId == currency.CurrencyId) ??
                        GetNewExchangeRate(currency);

                    Messenger.Default.Send(new NotificationMessage($"Updating currency {currency.Name}..."));

                    if (await ExchangeRateService.Update(exchangeRate) == Result.Fail)
                    {
                        Messenger.Default.Send(new NotificationMessage($"Could not update currency {currency.Name}",
                            NotificationStatusEnum.Error));
                    }
                }

                await RefreshData();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"{nameof(Update)}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public async Task UpdateAll()
        {
            IsBusy = true;

            try
            {
                var baseCurrencyId = FileSettings.BaseCurrency.CurrencyId;

                foreach (var currency in Data.Select(x => x.Currency).ToList())
                {
                    await SetBase(currency);
                    await Update();
                }

                await SetBase(Data.Select(x => x.Currency).Single(x => x.CurrencyId == baseCurrencyId));
                await Update();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"{nameof(UpdateAll)}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public async Task History(CurrenciesModel model)
        {
            var viewModel = ViewModelSource.Create<HistoricalDataViewModel>();
            viewModel.Model = model.ExchangeRate?.Rate ?? GetNewExchangeRate(model.Currency).Rate;

            var document = DocumentManagerService.CreateDocument(nameof(HistoricalDataView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                _fireChangeNotification = true;

                await RefreshData();
            }
        }

        [UsedImplicitly]
        public bool CanHistory(CurrenciesModel model) =>
            model?.Currency.CurrencyId != FileSettings.BaseCurrency.CurrencyId;

        private ExchangeRate GetNewExchangeRate(Currency currency)
        {
            return new()
            {
                FromCurrency = FileSettings.BaseCurrency, ToCurrency = currency,
                Rate = new data.Models.HistoricalData
                {
                    Name = $"Exchange rate {FileSettings.BaseCurrency.Name} => {currency.Name}"
                }
            };
        }

        [UsedImplicitly]
        public record CurrenciesModel(Currency BaseCurrency, Currency Currency, ExchangeRate ExchangeRate)
        {
            public DateTime? LatestDate => ExchangeRate?.Rate.LatestDate;

            public decimal? LatestValue => ExchangeRate?.Rate.LatestValue;

            public IEnumerable<decimal> History =>
                ExchangeRate?.Rate.DataPoints.OrderByDescending(x => x.Date).Select(x => x.Value);
        }
    }
}