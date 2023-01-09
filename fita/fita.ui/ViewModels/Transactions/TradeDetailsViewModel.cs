using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Common;
using JetBrains.Annotations;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Transactions;

[POCOViewModel]
public class TradeDetailsViewModel : ComposedDocumentViewModelBase, IDesiredSize, IHasSaved
{
    public int Width => 400;

    public int Height => 600;

    public Trade Trade { get; set; }
        
    public Transaction Transaction { get; set; }

    public Account Account { get; set; }

    public LockableCollection<Security> Securities { get; set; } = new();

    public virtual Security Security { get; set; }
        
    public bool Saved { get; private set; }

    [Import]
    public SecurityRepoService SecurityRepoService { get; set; }

    [Import]
    public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }
        
    [Import]
    public IPortfolioService PortfolioService { get; set; }

    [UsedImplicitly]
    public async Task RefreshData()
    {
        IsBusy = true;

        Securities.BeginUpdate();
        Securities.Clear();

        try
        {
            var securities = await SecurityRepoService.GetAll(true);
            Securities.AddRange(securities);

            Security = Securities.SingleOrDefault(x => x.SecurityId == Trade.Security?.SecurityId);
        }
        finally
        {
            Securities.EndUpdate();
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
            Trade.Security = Security;

            if (await PortfolioService.IsTradePossible(Trade) && await PortfolioService.ProcessTrade(Trade, Transaction))
            {
                Messenger.Default.Send(new NotificationMessage("Trade saved.", NotificationStatusEnum.Success));
                Saved = true;
            }
            else
            {
                Messenger.Default.Send(new NotificationMessage("Trade failed.", NotificationStatusEnum.Error));
            }
                
            DocumentOwner?.Close(this);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public bool CanSave() 
        => Trade.Quantity != 0m && Trade.Price > 0m;

    [UsedImplicitly]
    protected async void OnSecurityChanged(Security oldValue)
    {
        if (Trade is null || Account is null || Security is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            Trade.Price =
                (await SecurityHistoryRepoService.GetFromSecurity(Security))?.Price?.LatestValue ?? 0m;
            RaisePropertyChanged(() => Trade);
        }
        finally
        {
            IsBusy = false;
        }
    }
}