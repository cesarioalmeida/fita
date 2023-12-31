using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.CodeView;
using DevExpress.Xpf.Core;
using DryIoc;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Repositories;
using fita.ui.Common;
using fita.ui.ViewModels.Transactions.ImportManagers;
using JetBrains.Annotations;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions;

[POCOViewModel]
public class TransactionsImportViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
{
    public int Width => 1024;
    public int Height => 600;
    
    private IImportManager _importManager;

    public Account Account { get; set; }

    public virtual string ImportFilePath { get; set; }
    
    public virtual ObservableCollection<EntityModel> Entities { get; set; }
    
    public virtual LockableCollection<Category> Categories { get; set; } = new();
    
    public bool Saved { get; private set; }
        
    [Import]
    public CategoryRepoService CategoryRepoService { get; set; }
        
    [Import]
    public TransactionRepoService TransactionRepoService { get; set; }
    
    protected IOpenFileDialogService OpenFileDialogService => GetService<IOpenFileDialogService>();

    [UsedImplicitly]
    public async Task RefreshData()
    {
        IsBusy = true;

        try
        {
            Categories.BeginUpdate();
            Categories.Clear();
            
            var categories = await CategoryRepoService.GetAll();
            Categories.AddRange(categories.Where(c => 
                c.Group != CategoryGroupEnum.TransfersIn && c.Group != CategoryGroupEnum.TransfersOut
                                                         && c.Group != CategoryGroupEnum.TradeBuy && c.Group != CategoryGroupEnum.TradeSell));
            
            // retrieve import manager
            _importManager = DryIocBootstrapper.Container.ResolveMany<IImportManager>()
                            .SingleOrDefault(x => x.AppliesToAccountsWithName.Contains(Account.Name));
        }
        finally
        {
            Categories.EndUpdate();
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public void OpenFile()
    {
        OpenFileDialogService.Filter = _importManager.FileFilter;
        
        if (OpenFileDialogService.ShowDialog())
        {
            ImportFilePath = OpenFileDialogService.Files.First().GetFullName();
        }
        
        if (string.IsNullOrWhiteSpace(ImportFilePath))
        {
            return;
        }
        
        // load file and adjust transactions
        var importedTransactions = _importManager.GetTransactions(ImportFilePath, Categories).ToList();
        importedTransactions.ForEach(x => x.Transaction.AccountId = Account.AccountId);
        Entities = importedTransactions.Select(x => new EntityModel(x.IsSelected, x.Transaction)).ToObservableCollection();
    }

    [UsedImplicitly]
    public void Cancel() 
        => DocumentOwner?.Close(this);

    [UsedImplicitly]
    public async Task Import()
    {
        IsBusy = true;

        try
        {
            foreach (var entity in Entities.Where(x => x.IsSelected))
            {
                Messenger.Default.Send(await TransactionRepoService.Save(entity.Entity) == Result.Fail
                    ? new NotificationMessage("Failed to save transaction.", NotificationStatusEnum.Error)
                    : new NotificationMessage($"Transaction {entity.Entity.Description} saved.", NotificationStatusEnum.Success));
                
                Saved = true;
            }
            
            DocumentOwner?.Close(this);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public bool CanImport()
        => Entities?.Any(x => x.IsSelected) ?? false;
    
    public class EntityModel(bool isSelected, Transaction entity)
    {
        public bool IsSelected { get; set; } = isSelected;

        public Transaction Entity { get; set; } = entity;
    }
}