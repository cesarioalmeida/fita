using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using System.Linq;
using fita.ui.DisplayModels;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Interfaces;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels
{
    [POCOViewModel]
    public class ShellViewModel : ComposedViewModelBase
    {
        public ShellViewModel()
        {
            Messenger.Default.Register<DisplayModelMessage>(this, this.OnDisplayModelMessage);
        }

        public IEnumerable<IIsModelView> AvailableDocumentViews { get; set; }

        // public virtual ObservableCollection<Account> Accounts {get; set;}

        // public Account SelectedAccount {get; set;}

        protected virtual IDocumentManagerService DocumentManagerService => this.GetRequiredService<IDocumentManagerService>();

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

        public async void AddAccount()
        {
            // var account = new Account();

            //SelectedAccount = account;
        }

        public void Categories()
        {
            //Messenger.Default.Send(new DisplayModelMessage(new Categories()));
        }

        public void Currencies()
        {
            Messenger.Default.Send(new DisplayModelMessage(new ListCurrencies()));
        }

        public void Settings()
        {
            //Messenger.Default.Send(new DisplayModelMessage(new ListCurrencies()));
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