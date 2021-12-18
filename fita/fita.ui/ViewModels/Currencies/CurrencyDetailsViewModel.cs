using System;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Common;
using JetBrains.Annotations;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Currencies
{
    [POCOViewModel]
    public class CurrencyDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
    {
        public int Width => 400;

        public int Height => 600;

        public CurrencyRepoService CurrencyRepoService { get; set; }
        
        public LoggingService LoggingService { get; set; }

        public Currency Currency { get; set; }

        public bool Saved { get; private set; }

        [UsedImplicitly]
        public void Cancel() => DocumentOwner?.Close(this);

        [UsedImplicitly]
        public async Task Save()
        {
            IsBusy = true;

            try
            {
                Messenger.Default.Send(await CurrencyRepoService.SaveAsync(Currency) == Result.Fail
                    ? new NotificationMessage("Failed to save currency.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Currency {Currency.Name} saved.", NotificationStatusEnum.Success));

                Saved = true;
                DocumentOwner?.Close(this);
            }
            catch (Exception ex)
            {
                LoggingService.Error($@"{nameof(CurrencyDetailsViewModel)} - {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}