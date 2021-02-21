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
using fita.services.Repositories;
using fita.ui.Views.Accounts;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Accounts
{
    [POCOViewModel]
    public class AccountsViewModel : ComposedDocumentViewModelBase
    {
        private bool fireChangeNotification;

        public AccountRepoService AccountRepoService { get; set; }

        protected virtual IDocumentManagerService DocumentManagerService => null;

        public virtual LockableCollection<EntityModel> Data { get; set; } = new();

        public void Close()
        {
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
                var accounts = await AccountRepoService.AllEnrichedAsync();

                var data = accounts.Select(s => new EntityModel(s));

                Data.AddRange(data);
            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        public async Task Edit(Account account)
        {
            var viewModel = ViewModelSource.Create<AccountDetailsViewModel>();
            viewModel.Entity = account ?? new Account();

            var document =
                this.DocumentManagerService.CreateDocument(nameof(AccountDetailsView), viewModel, null, this);
            document.DestroyOnClose = true;
            document.Show();

            if (viewModel.Saved)
            {
                fireChangeNotification = true;

                await RefreshData();
            }
        }

        public async Task Delete(Account account)
        {
            if (account == null)
            {
                return;
            }

            if (WinUIMessageBox.Show(
                $"Are you sure you want to delete the account {account.Name}?",
                "Delete account",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Messenger.Default.Send(await AccountRepoService.DeleteAsync(account.AccountId) == Result.Fail
                    ? new NotificationMessage("Failed to delete account.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Security {account.Name} deleted.", NotificationStatusEnum.Success));

                await RefreshData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public class EntityModel
        {
            public EntityModel(Account entity)
            {
                Entity = entity;
            }

            public Account Entity { get; }
        }
    }
}