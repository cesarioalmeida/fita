using DevExpress.Mvvm.DataAnnotations;
using fita.core.DTOs;
using fita.core.Models;
using fita.ui.Services;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm.POCO;
using LiteDB;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels
{
    [POCOViewModel]
    public class CurrenciesViewModel : ComposedDocumentViewModelBase
    {
        public override object Title { get; set; } = "Currencies";
        public virtual ObservableCollection<Currency> Currencies { get; set; }

        public virtual Currency SelectedCurrency { get; set; }

        [ServiceProperty(Key = "NameFocusService")]
        public virtual IFocusService NameFocusService => null;

        public ExtendedPersistenceService ExtendedPersistenceService { get; set; }

        public void Cancel()
        {
            Currencies.Clear();
            DocumentOwner?.Close(this);
        }

        public async void Ok()
        {
            IsBusy = true;

            try
            {
                foreach (var currency in Currencies)
                {
                    // ignore currencies already defined
                    if (currency.SavedState == null && Currencies.Except(new[] {currency}).Select(x => x.Name.ToUpper())
                        .Contains(currency.Name.ToUpper()))
                    {
                        continue;
                    }

                    await ExtendedPersistenceService.SaveCurrency(currency);
                }
            }
            finally
            {
                IsBusy = false;
            }

            DocumentOwner?.Close(this);
        }

        public bool CanOk()
        {
            return Currencies.All(x => !x.HasError(_ => _.Name) && !x.HasError(_ => _.Symbol));
        }

        public async void RefreshData()
        {
            IsBusy = true;

            Currencies = new ObservableCollection<Currency>();

            try
            {
                var currencies = await PersistenceService.GetAllAsync<Currency, CurrencyDTO>();

                if (currencies != null)
                {
                    foreach (var currency in currencies.Where(x => x.HistoricalDataId != ObjectId.Empty))
                    {
                        currency.ExchangeData =
                            await PersistenceService.GetFromIdAsync<HistoricalData, HistoricalDataDTO>(
                                currency.HistoricalDataId) ??
                            new HistoricalData();

                        currencies.ForEach(x => x.SaveState());
                        Currencies.Add(currency);
                    }
                }

                SelectedCurrency = currencies?.FirstOrDefault();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Add()
        {
            var currency = new Currency();
            Currencies.Add(currency);
            SelectedCurrency = currency;

            NameFocusService?.Focus();
        }
    }
}