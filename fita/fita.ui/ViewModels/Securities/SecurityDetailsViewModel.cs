﻿using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services;
using fita.services.Repositories;
using fita.ui.Common;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Securities
{
    [POCOViewModel]
    public class SecurityDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize
    {
        public int Width => 400;

        public int Height => 600;

        public SecurityRepoService SecurityRepoService { get; set; }

        public CurrencyRepoService CurrencyRepoService { get; set; }

        public Security Entity { get; set; }

        public Currency SelectedCurrency { get; set; }

        public LockableCollection<Currency> Currencies { get; set; } = new();

        public bool Saved { get; private set; }

        public async Task RefreshData()
        {
            IsBusy = true;

            Currencies.BeginUpdate();
            Currencies.Clear();

            try
            {
                var currencies = await CurrencyRepoService.AllAsync();
                Currencies.AddRange(currencies);

                SelectedCurrency = Currencies.SingleOrDefault(x => x.CurrencyId == Entity.Currency?.CurrencyId);
            }
            finally
            {
                Currencies.EndUpdate();
                IsBusy = false;
            }
        }

        public void Cancel()
        {
            DocumentOwner?.Close(this);
        }
        
        public async Task Save()
        {
            IsBusy = true;

            try
            {
                Entity.Currency = SelectedCurrency;

                Messenger.Default.Send(await SecurityRepoService.SaveAsync(Entity) == Result.Fail
                    ? new NotificationMessage("Failed to save security.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Security {Entity.Name} saved.", NotificationStatusEnum.Success));

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