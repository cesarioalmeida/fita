using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using fita.core.DTOs;
using fita.core.Models;
using fita.ui.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Interfaces;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels
{
    [POCOViewModel]
    public class DashboardViewModel : ComposedViewModelBase
    {
        public DashboardViewModel()
        {
            Messenger.Default.Register<DisplayModelMessage>(this, this.OnDisplayModelMessage);
        }

        public IEnumerable<IIsModelView> AvailableDocumentViews { get; set; }

        public virtual ObservableCollection<Account> Accounts {get; set;}

        public Account SelectedAccount {get; set;}

        protected virtual IDocumentManagerService DocumentManagerService => this.GetRequiredService<IDocumentManagerService>();

        public async void RefreshData()
        {
            IsBusy = true;

            try
            {
                var accounts = await this.PersistenceService.GetAllAsync<Account, AccountDTO>();
                //SelectedAccount = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void AddAccount()
        {
            var account = new Account();

            //SelectedAccount = account;
        }

        public void Categories()
        {
            Messenger.Default.Send(new DisplayModelMessage(new Currencies()));
        }

        public void Currencies()
        {
            Messenger.Default.Send(new DisplayModelMessage(new Currencies()));
        }

        public void Settings()
        {
            Messenger.Default.Send(new DisplayModelMessage(new Currencies()));
        }

        private void OnDisplayModelMessage(DisplayModelMessage obj)
        {
            var documentViewType = this.AvailableDocumentViews.FirstOrDefault(x => x.ModelType == obj.Model.GetType());

            if (documentViewType == null)
            {
                return;
            }

            var document = this.DocumentManagerService.CreateDocument(documentViewType.View, obj.Model, this);
            document.DestroyOnClose = true;

            document.Show();
        }
    }
}