using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Timers;
using System.Windows;
using DevExpress.Xpf.WindowsUI;
using DryIoc;
using fita.data.Enums;
using fita.data.Models;
using fita.services.Repositories;
using fita.ui.Messages;
using JetBrains.Annotations;
using LiteDB;
using twentySix.Framework.Core.Helpers;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.Services;
using twentySix.Framework.Core.Services.Interfaces;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels;

[POCOViewModel]
public class ShellViewModel : ComposedViewModelBase, IDisposable
{
    private static readonly Dictionary<AccountTypeEnum, string> AccountToGlyphMapper =
        new()
        {
            {AccountTypeEnum.Bank, "../Resources/Icons/Bank_24x24.png"},
            {AccountTypeEnum.CreditCard, "../Resources/Icons/CreditCard_24x24.png"},
            {AccountTypeEnum.Investment, "../Resources/Icons/Investment_24x24.png"},
            {AccountTypeEnum.Asset, "../Resources/Icons/Asset_24x24.png"},
        };

    private readonly Timer _messageTimer = new();

    private List<HamburgerMenuItemViewModel> _accountItems = new();
    private IDBHelperService _dbHelperService;

    [Import] public AccountRepoService AccountRepoService { get; set; }

    public virtual string NotificationMessage { get; set; }

    public HamburgerMenuItemViewModel SelectedHamburgerItem { get; set; }

    public IEnumerable<HamburgerMenuItemViewModel> HomeHamburgerItem => new[]
    {
        new HamburgerMenuItemViewModel("Home", "HomeView", "../Resources/Icons/Home_24x24.png") {IsChecked = true}
    };

    public IEnumerable<HamburgerMenuItemViewModel> BankHamburgerItems =>
        _accountItems.Where(x => x.Account.Type == AccountTypeEnum.Bank);

    public IEnumerable<HamburgerMenuItemViewModel> CreditCardHamburgerItems =>
        _accountItems.Where(x => x.Account.Type == AccountTypeEnum.CreditCard);

    public IEnumerable<HamburgerMenuItemViewModel> InvestmentHamburgerItems =>
        _accountItems.Where(x => x.Account.Type == AccountTypeEnum.Investment);

    public IEnumerable<HamburgerMenuItemViewModel> AssetHamburgerItems =>
        _accountItems.Where(x => x.Account.Type == AccountTypeEnum.Asset);

    public IEnumerable<HamburgerMenuItemViewModel> ReportHamburgerItems => new[]
    {
        new HamburgerMenuItemViewModel("All Transactions", "AllTransactionsReportView",
            "../Resources/Icons/Reports_24x24.png"),
        new HamburgerMenuItemViewModel("Closed Positions", "ClosedPositionsReportView",
            "../Resources/Icons/Reports_24x24.png"),
        new HamburgerMenuItemViewModel("Income/Expenses", "IncomeExpensesReportView",
            "../Resources/Icons/Reports_24x24.png"),
        new HamburgerMenuItemViewModel("PL (Month)", "PLMonthReportView", "../Resources/Icons/Reports_24x24.png"),
        new HamburgerMenuItemViewModel("Category (Month)", "CategoryEvolutionReportView",
            "../Resources/Icons/Reports_24x24.png"),
        new HamburgerMenuItemViewModel("YoY Category", "YoYCategoryReportView", "../Resources/Icons/Reports_24x24.png"),
        new HamburgerMenuItemViewModel("Net Worth", "NetWorthReportView", "../Resources/Icons/Reports_24x24.png")
    };

    protected IDocumentManagerService ModalDocumentService =>
        this.GetRequiredService<IDocumentManagerService>("ModalWindowDocumentService");

    protected IDispatcherService DispatcherService => GetService<IDispatcherService>();

    public ShellViewModel()
    {
        Messenger.Default.Register<NotificationMessage>(this, OnNotificationMessage);
        Messenger.Default.Register<AccountsChanged>(this, _ => { RefreshData().ConfigureAwait(false); });
        Messenger.Default.Register<SecuritiesChanged>(this, _ => { RefreshData().ConfigureAwait(false); });

        _messageTimer.Interval = 5000;
        _messageTimer.Elapsed += (_, _) => DispatcherService.BeginInvoke(() =>
        {
            NotificationMessage = null;
            _messageTimer.Stop();
        });
    }

    public async Task RefreshData()
    {
        IsBusy = true;

        try
        {
            var selectedId = ObjectId.Empty;

            if (SelectedHamburgerItem != null)
            {
                selectedId = SelectedHamburgerItem.Account.AccountId;
            }

            var accounts = (await AccountRepoService.GetAll(true)).ToList();

            if (accounts.Any())
            {
                _accountItems = accounts.Select(_ =>
                        new HamburgerMenuItemViewModel(_.Name, "TransactionsView", AccountToGlyphMapper[_.Type],
                            _))
                    .ToList();

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

    [UsedImplicitly]
    public void ShowModalView(string view)
    {
        var document = ModalDocumentService.CreateDocument(view, null, this);
        document.DestroyOnClose = true;
        document.Show();
    }

    [UsedImplicitly]
    public async Task OnViewLoaded()
    {
        _dbHelperService = ApplicationHelper.Container.Resolve<DBHelperServiceFactory>()?.GetInstance();

        await RefreshData();

        NavigationService?.Navigate("HomeView", null, this);
    }

    [UsedImplicitly]
    public void Navigate(HamburgerMenuItemViewModel hamburgerItem)
        => NavigationService?.Navigate(hamburgerItem.View, hamburgerItem.Account, this);

    [UsedImplicitly]
    public void BackupDb()
    {
        if (string.IsNullOrEmpty(_dbHelperService?.DBLocation))
        {
            return;
        }

        var dbFile = _dbHelperService.DBLocation;
        var backupFile = Path.Join(Path.GetDirectoryName(dbFile),
            Path.GetFileNameWithoutExtension(dbFile) + "-backup" + Path.GetExtension(dbFile));

        try
        {
            File.Copy(dbFile!, backupFile, true);

            WinUIMessageBox.Show(
                "The data was backed up successfully.",
                "Backup DB",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            WinUIMessageBox.Show(
                $"There was a problem while backing up the data. {ex.Message}",
                "Backup DB",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void Dispose()
    {
        _messageTimer?.Dispose();
        _dbHelperService?.Dispose();
    }

    private void OnNotificationMessage(NotificationMessage obj)
    {
        NotificationMessage = $"[{obj.Status}] {obj.Message}";
        _messageTimer.Start();
    }

    public record HamburgerMenuItemViewModel(string Caption, string View, string Icon = null,
        Account Account = null, bool IsChecked = false);
}