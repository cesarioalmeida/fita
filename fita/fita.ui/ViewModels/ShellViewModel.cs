using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using fita.ui.Views.Categories;
using fita.ui.Views.Currencies;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.Interfaces;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels
{
    [POCOViewModel]
    public class ShellViewModel : ComposedViewModelBase, IDependsOnClose
    {
        private Timer _messageTimer = new();

        public virtual NotificationMessage Message { get; set; }

        public virtual IEnumerable<IIsModelView> AvailableDocumentViews { get; set; }

        protected virtual IDocumentManagerService DocumentManagerService => this.GetRequiredService<IDocumentManagerService>();

        protected IDispatcherService DispatcherService => this.GetService<IDispatcherService>();

        public ShellViewModel()
        {
            Messenger.Default.Register<DisplayModelMessage>(this, this.OnDisplayModelMessage);
            Messenger.Default.Register<NotificationMessage>(this, this.OnNotificationMessage);

            _messageTimer.Interval = 5000;
            _messageTimer.Elapsed += (_, _) => DispatcherService.BeginInvoke(() => Message = null);
        }

        public async void RefreshData()
        {
            IsBusy = true;

            try
            {
                //var accounts = await this.PersistenceService.GetAllAsync<Account, AccountDTO>();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ShowView(string view)
        {
            var document = this.DocumentManagerService.CreateDocument(view, null, this);
            document.DestroyOnClose = true;
            document.Show();
        }

        public void Settings()
        {
            //Messenger.Default.Send(new DisplayModelMessage(new ListCurrencies()));
        }

        public void OnClose()
        {
            _messageTimer?.Stop();
            _messageTimer = null;
        }

        private void OnDisplayModelMessage(DisplayModelMessage obj)
        {
            var documentViewType = this.AvailableDocumentViews.FirstOrDefault(x => x.ModelType == obj.Model?.GetType());

            if (documentViewType == null)
            {
                return;
            }

            var document = this.DocumentManagerService.CreateDocument(documentViewType.View, obj.Model, this);
            document.DestroyOnClose = true;

            document.Show();
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
    }
}