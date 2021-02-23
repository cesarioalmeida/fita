using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Timers;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Repositories;
using LiteDB;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.Interfaces;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels
{
    [POCOViewModel]
    public class ShellViewModel : ComposedViewModelBase, IDependsOnClose
    {
        private static readonly Dictionary<AccountTypeEnum, string> _accountToGlyphMapper =
            new()
            {
                {AccountTypeEnum.Bank, "../Resources/Icons/Bank_24x24.png"},
                {AccountTypeEnum.CreditCard, "../Resources/Icons/CreditCard_24x24.png"},
                {AccountTypeEnum.Investment, "../Resources/Icons/Investment_24x24.png"},
                {AccountTypeEnum.Asset, "../Resources/Icons/Asset_24x24.png"},
            };

        private Timer _messageTimer = new();

        private List<HamburgerMenuItemViewModel> _accountItems = new();

        public NotificationMessage Message { get; set; }

        public HamburgerMenuItemViewModel SelectedHamburgerItem { get; set; }

        public IEnumerable<HamburgerMenuItemViewModel> BankHamburgerItems =>
            _accountItems.Where(x => x.Account.Type == AccountTypeEnum.Bank);

        public IEnumerable<HamburgerMenuItemViewModel> CreditCardHamburgerItems =>
            _accountItems.Where(x => x.Account.Type == AccountTypeEnum.CreditCard);

        public IEnumerable<HamburgerMenuItemViewModel> InvestmentHamburgerItems =>
            _accountItems.Where(x => x.Account.Type == AccountTypeEnum.Investment);

        public IEnumerable<HamburgerMenuItemViewModel> AssetHamburgerItems =>
            _accountItems.Where(x => x.Account.Type == AccountTypeEnum.Asset);

        protected IDocumentManagerService ModalDocumentService =>
            this.GetRequiredService<IDocumentManagerService>("ModalWindowDocumentService");

        protected IDocumentManagerService FrameDocumentService =>
            this.GetRequiredService<IDocumentManagerService>("FrameDocumentService");

        protected IDispatcherService DispatcherService => this.GetService<IDispatcherService>();

        protected AccountRepoService AccountRepoService { get; set; }

        public ShellViewModel()
        {
            Messenger.Default.Register<NotificationMessage>(this, this.OnNotificationMessage);

            _messageTimer.Interval = 5000;
            _messageTimer.Elapsed += (_, _) => DispatcherService.BeginInvoke(() => Message = null);
        }

        public async void RefreshData()
        {
            IsBusy = true;

            try
            {
                ObjectId selectedId = ObjectId.Empty;

                if(SelectedHamburgerItem != null)
                {
                    selectedId = SelectedHamburgerItem.Account.AccountId;
                }

                var accounts = (await AccountRepoService.AllEnrichedAsync()).ToList();

                if (accounts.Any())
                {
                    _accountItems = accounts.Select(_ => 
                    new HamburgerMenuItemViewModel(_.Name, "AccountTransactionsView", _accountToGlyphMapper[_.Type], _)).ToList();

                    RaisePropertiesChanged(
                        () => BankHamburgerItems, 
                        () => CreditCardHamburgerItems,
                        () => InvestmentHamburgerItems,
                        () => AssetHamburgerItems);

                    SelectedHamburgerItem = _accountItems.SingleOrDefault(x => x.Account.AccountId == selectedId);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ShowView(string view)
        {
            var document = ModalDocumentService.CreateDocument(view, null, this);
            document.DestroyOnClose = true;
            document.Show();
        }

        public void OnClose()
        {
            _messageTimer?.Stop();
            _messageTimer = null;
        }

        private void OnNotificationMessage(NotificationMessage obj)
        {
            Message = obj;

            if (obj.Status != NotificationStatusEnum.Error)
            {
                _messageTimer.Start();
            }
            else
            {
                _messageTimer.Stop();
            }
        }

        public class HamburgerMenuItemViewModel
        {
            public string Caption { get; set; }

            public string Icon { get; set; }

            public string View { get; set; }

            public Account Account { get; set; }

            public HamburgerMenuItemViewModel(string caption, string view, string icon = null, Account account = null)
            {
                Caption = caption;
                View = view;
                Icon = icon;
                Account = account;
            }
        }
    }
}