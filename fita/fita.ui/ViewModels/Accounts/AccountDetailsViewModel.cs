using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Repositories;
using fita.ui.Common;
using JetBrains.Annotations;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Accounts
{
    [POCOViewModel]
    public class AccountDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
    {
        public int Width => 400;

        public int Height => 600;

        [Import]
        public AccountRepoService AccountRepoService { get; set; }

        [Import]
        public CurrencyRepoService CurrencyRepoService { get; set; }

        public Account Entity { get; set; }

        public Currency SelectedCurrency { get; set; }

        public LockableCollection<Currency> Currencies { get; set; } = new();

        public bool Saved { get; private set; }

        [UsedImplicitly]
        public async Task RefreshData()
        {
            IsBusy = true;

            Currencies.BeginUpdate();
            Currencies.Clear();

            try
            {
                var currencies = await CurrencyRepoService.GetAll();
                Currencies.AddRange(currencies);

                SelectedCurrency = Currencies.SingleOrDefault(x => x.CurrencyId == Entity.Currency?.CurrencyId);
            }
            finally
            {
                Currencies.EndUpdate();
                IsBusy = false;
            }
        }

        [UsedImplicitly]
        public void Cancel() => DocumentOwner?.Close(this);

        [UsedImplicitly]
        public async Task Save()
        {
            IsBusy = true;

            try
            {
                Entity.Currency = SelectedCurrency;

                Messenger.Default.Send(await AccountRepoService.Save(Entity) == Result.Fail
                    ? new NotificationMessage("Failed to save account.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Account {Entity.Name} saved.", NotificationStatusEnum.Success));

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