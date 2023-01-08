using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Repositories;
using fita.ui.Common;
using JetBrains.Annotations;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions;

[POCOViewModel]
public class TransactionDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
{
    public int Width => 400;

    public int Height => 600;

    public Transaction Entity { get; set; }
        
    public Account Account { get; set; }

    public LockableCollection<Category> Categories { get; set; } = new();

    public virtual Category SelectedCategory { get; set; }

    public bool Saved { get; private set; }
        
    [Import]
    public CategoryRepoService CategoryRepoService { get; set; }
        
    [Import]
    public TransactionRepoService TransactionRepoService { get; set; }
        
    public virtual bool IsPayment { get; set; }

    [UsedImplicitly]
    public async Task RefreshData()
    {
        IsBusy = true;

        Categories.BeginUpdate();
        Categories.Clear();

        try
        {
            var categories = await CategoryRepoService.GetAll();
            Categories.AddRange(categories.Where(c => 
                c.Group != CategoryGroupEnum.TransfersIn && c.Group != CategoryGroupEnum.TransfersOut
                                                         && c.Group != CategoryGroupEnum.TradeBuy && c.Group != CategoryGroupEnum.TradeSell));

            SelectedCategory = Categories.SingleOrDefault(x => x.CategoryId == Entity.Category?.CategoryId);
        }
        finally
        {
            Categories.EndUpdate();
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public void Cancel() 
        => DocumentOwner?.Close(this);

    [UsedImplicitly]
    public async Task Save()
    {
        IsBusy = true;

        try
        {
            Entity.Category = SelectedCategory;

            if (Entity.Deposit is 0m)
            {
                Entity.Deposit = null;
            }
                 
            if (Entity.Payment is 0m)
            {
                Entity.Payment = null;
            }
                
            Messenger.Default.Send(await TransactionRepoService.Save(Entity) == Result.Fail
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

    [UsedImplicitly]
    public bool CanSave() 
        => Entity.Deposit.HasValue || Entity.Payment.HasValue;

    [UsedImplicitly]
    protected void OnSelectedCategoryChanged(Category oldCategory) 
        => IsPayment = SelectedCategory.Group is CategoryGroupEnum.PersonalExpenses;
}