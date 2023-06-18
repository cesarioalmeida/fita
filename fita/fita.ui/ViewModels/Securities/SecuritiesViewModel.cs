using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using fita.data.Models;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.Messages;
using fita.ui.ViewModels.HistoricalData;
using fita.ui.Views.HistoricalData;
using fita.ui.Views.Securities;
using JetBrains.Annotations;
using twentySix.Framework.Core.Common;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Securities;

[POCOViewModel]
public class SecuritiesViewModel : ComposedDocumentViewModelBase
{
    private bool _fireChangeNotification;
    
    [Import]
    public SecurityRepoService SecurityRepoService { get; set; }

    [Import]
    public SecurityHistoryRepoService SecurityHistoryRepoService { get; set; }

    [Import]
    public HistoricalDataRepoService HistoricalDataRepoService { get; set; }

    [Import]
    public ISecurityService SecurityService { get; set; }

    protected IDocumentManagerService DocumentManagerService =>
        this.GetRequiredService<IDocumentManagerService>("ModalWindowDocumentService");

    public virtual LockableCollection<EntityModel> Data { get; set; } = new();

    [UsedImplicitly]
    public void Close()
    {
        if (_fireChangeNotification)
        {
            Messenger.Default.Send(new SecuritiesChanged());
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
            var securities = await SecurityRepoService.GetAll(true);
            var securityHistories = await SecurityHistoryRepoService.GetAll(true);

            var data = securities.Select(s =>
                new EntityModel(s,
                    securityHistories.FirstOrDefault(x => x.Security.SecurityId == s.SecurityId)));

            Data.AddRange(data);
        }
        catch (Exception)
        {
            Messenger.Default.Send(new NotificationMessage("Failed to refresh securities", NotificationStatusEnum.Error));
        }
        finally
        {
            Data.EndUpdate();
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public async Task Edit(Security security)
    {
        var viewModel = ViewModelSource.Create<SecurityDetailsViewModel>();
        viewModel.Entity = security ?? new Security();

        var document = DocumentManagerService.CreateDocument(nameof(SecurityDetailsView), viewModel, null, this);
        document.DestroyOnClose = true;
        document.Show();

        if (viewModel.Saved)
        {
            _fireChangeNotification = true;
            await RefreshData();
        }
    }

    [UsedImplicitly]
    public async Task Delete(Security security)
    {
        if (security is null)
        {
            return;
        }

        if (WinUIMessageBox.Show(
                $"Are you sure you want to delete the security {security.Name}?",
                "Delete security",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) != MessageBoxResult.OK)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var historicalToDelete = await SecurityHistoryRepoService.GetFromSecurity(security);

            if (historicalToDelete is not null)
            {
                if (historicalToDelete.Price is not null)
                {
                    await HistoricalDataRepoService.Delete(historicalToDelete.Price.HistoricalDataId);
                }

                await SecurityHistoryRepoService.Delete(historicalToDelete.SecurityHistoryId);
            }

            Messenger.Default.Send(await SecurityRepoService.Delete(security.SecurityId) == Result.Fail
                ? new NotificationMessage("Failed to delete security.", NotificationStatusEnum.Error)
                : new NotificationMessage($"Security {security.Name} deleted.", NotificationStatusEnum.Success));

            await RefreshData();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public async Task Update()
    {
        IsBusy = true;

        try
        {
            foreach (var data in Data)
            {
                var securityHistory = data.EntityHistory ?? await GetNewSecurityHistory(data.Entity);

                Messenger.Default.Send(new NotificationMessage($"Updating security {data.Entity.Name}..."));

                if (await SecurityService.Update(securityHistory) == Result.Fail)
                {
                    Messenger.Default.Send(new NotificationMessage($"Could not update security {data.Entity.Name}",
                        NotificationStatusEnum.Error));
                }
            }

            await RefreshData();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [UsedImplicitly]
    public async Task History(EntityModel model)
    {
        var viewModel = ViewModelSource.Create<HistoricalDataViewModel>();
        viewModel.Model = model.EntityHistory?.Price ?? (await GetNewSecurityHistory(model.Entity)).Price;

        var document = DocumentManagerService.CreateDocument(nameof(HistoricalDataView), viewModel, null, this);
        document.DestroyOnClose = true;
        document.Show();

        if (viewModel.Saved)
        {
            _fireChangeNotification = true;
            await RefreshData();
        }
    }

    private async Task<SecurityHistory> GetNewSecurityHistory(Security security)
    {
        var securityHistory = new SecurityHistory
        {
            Security = security,
            Price = new data.Models.HistoricalData
            {
                Name = $"Price History for {security.Name}"
            }
        };

        await SecurityHistoryRepoService.Save(securityHistory);

        return securityHistory;
    }

    public record EntityModel(Security Entity, SecurityHistory EntityHistory)
    {
        public DateTime? LatestDate => EntityHistory?.Price?.LatestDate;

        public decimal? LatestValue => EntityHistory?.Price?.LatestValue;

        public IEnumerable<decimal> History => EntityHistory?.Price?.DataPoints.OrderByDescending(x => x.Date).Select(x => x.Value);
    }
}