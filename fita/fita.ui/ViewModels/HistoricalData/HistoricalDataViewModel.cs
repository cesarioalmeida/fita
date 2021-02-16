using System;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.WindowsUI;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Common;
using fita.ui.Services;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.HistoricalData
{
    [POCOViewModel]
    public class HistoricalDataViewModel : ComposedDocumentViewModelBase, IDesiredSize
    {
        public int Width => 400;

        public int Height => 600;

        public override object Title { get; set; }

        public virtual data.Models.HistoricalData Model { get; set; }

        public bool Saved { get; private set; }

        public HistoricalDataService HistoricalDataService { get; set; }

        protected virtual IGridControlService GridControlService => null;

        public void Close()
        {
            DocumentOwner?.Close(this);
        }

        public void Edit(HistoricalPoint historical)
        {

        }

        public async Task Delete(HistoricalPoint historical)
        {
            if (historical == null)
            {
                return;
            }

            if (WinUIMessageBox.Show(
                $"Are you sure you want to delete the date {historical.Date.ToShortDateString()}?",
                "Delete Date",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Model.Data.Remove(historical.Date);

                Messenger.Default.Send(await HistoricalDataService.SaveAsync(Model) == Result.Fail
                    ? new NotificationMessage("Failed to delete historical point.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Historical point {historical.Date.ToShortDateString()} deleted.", NotificationStatusEnum.Success));

                Saved = true;

                RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
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

        private void RefreshData()
        {
            GridControlService?.Refresh();
        }
    }
}