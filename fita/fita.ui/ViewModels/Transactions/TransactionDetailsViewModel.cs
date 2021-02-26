using System.Linq;
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

namespace fita.ui.ViewModels.Transactions
{
    [POCOViewModel]
    public class TransactionDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize
    {
        public int Width => 400;

        public int Height => 600;

        public Transaction Entity { get; set; }
        
        public Account Account { get; set; }

        public LockableCollection<Category> Categories { get; set; } = new();
        
        public Category SelectedCategory { get; set; }

        public bool Saved { get; private set; }
        
        public CategoryRepoService CategoryRepoService { get; set; }
        
        public TransactionRepoService TransactionRepoService { get; set; }

        public async Task RefreshData()
        {
            IsBusy = true;

            Categories.BeginUpdate();
            Categories.Clear();

            try
            {
                var categories = await CategoryRepoService.AllAsync();
                Categories.AddRange(categories);

                 SelectedCategory = Categories.SingleOrDefault(x => x.CategoryId == Entity.Category?.CategoryId);
            }
            finally
            {
                Categories.EndUpdate();
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
                 Entity.Category = SelectedCategory;
                
                 Messenger.Default.Send(await TransactionRepoService.SaveAsync(Entity) == Result.Fail
                     ? new NotificationMessage("Failed to save transaction.", NotificationStatusEnum.Error)
                     : new NotificationMessage($"Transaction {Entity.Description} saved.", NotificationStatusEnum.Success));

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