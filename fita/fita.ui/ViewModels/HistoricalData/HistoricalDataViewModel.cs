using System;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.WindowsUI;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Common;
using fita.ui.Services;
using fita.ui.Views.HistoricalData;
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

        public virtual data.Models.HistoricalData Model { get; set; }

        public bool Saved { get; private set; }

        public HistoricalDataRepoService HistoricalDataRepoService { get; set; }

        protected virtual IGridControlService GridControlService => null;

        protected virtual IDocumentManagerService DocumentManagerService => null;

        public void Close()
        {
            DocumentOwner?.Close(this);
        }

        public async Task Edit(HistoricalDataPoint historical)
        {
            if (historical == null)
            {
                historical = new HistoricalDataPoint {Date = DateTime.Now, Value = 0m};
                Model.DataPoints.Add(historical);
            }

            var viewModel = ViewModelSource.Create<HistoricalPointViewModel>();
            viewModel.Point = historical;

            var document = this.DocumentManagerService.CreateDocument(nameof(HistoricalPointView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                Messenger.Default.Send(await HistoricalDataRepoService.SaveAsync(Model) == Result.Fail
                    ? new NotificationMessage("Failed to edit historical point.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Historical point {historical.Date.ToShortDateString()} edited.", NotificationStatusEnum.Success));

                Saved = true;

                RefreshData();
            }
        }

        public async Task Delete(HistoricalDataPoint historical)
        {
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
                Model.DataPoints.Remove(historical);

                Messenger.Default.Send(await HistoricalDataRepoService.SaveAsync(Model) == Result.Fail
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
        
        private void RefreshData()
        {
            GridControlService?.Refresh();
        }
    }
}