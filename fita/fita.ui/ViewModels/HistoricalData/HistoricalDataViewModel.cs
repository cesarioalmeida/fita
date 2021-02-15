using System.Collections.Generic;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using fita.services.Repositories;
using fita.ui.Common;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.HistoricalData
{
    [POCOViewModel]
    public class HistoricalDataViewModel : ComposedDocumentViewModelBase, IDesiredSize
    {
        public int Width => 400;

        public int Height => 600;

        public override object Title { get; set; }

        public data.Models.HistoricalData Model { get; set; }

        public bool Saved { get; private set; }

        public HistoricalDataService HistoricalDataService { get; set; }

        public void Cancel()
        {
            DocumentOwner?.Close(this);
        }

        public void Edit(data.Models.HistoricalPoint historical)
        {

        }

        public void Delete()
        {
        }
        
        public async Task Save()
        {
            IsBusy = true;

            try
            {
                //Messenger.Default.Send(await CurrencyService.SaveAsync(Currency) == Result.Fail
                //    ? new NotificationMessage("Failed to save currency.", NotificationStatusEnum.Error)
                //    : new NotificationMessage($"Currency {Currency.Name} saved.", NotificationStatusEnum.Success));

                Saved = true;
                DocumentOwner?.Close(this);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}