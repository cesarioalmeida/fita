using System;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Common;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Currencies
{
    [POCOViewModel]
    public class CurrencyDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize
    {
        public int Width => 400;

        public int Height => 600;

        public override object Title { get; set; }

        public CurrencyService CurrencyService { get; set; }

        public Currency Currency { get; set; }

        public bool Saved { get; private set; }

        public void Cancel()
        {
            DocumentOwner?.Close(this);
        }
        
        public async Task Save()
        {
            IsBusy = true;

            try
            {
                Messenger.Default.Send(await CurrencyService.SaveAsync(Currency) == Result.Fail
                    ? new NotificationMessage("Failed to save currency.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Currency {Currency.Name} saved.", NotificationStatusEnum.Success));

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