using System.Threading.Tasks;
using DevExpress.Mvvm;
using fita.data.Models;
using fita.ui.Messages;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Reports;

public abstract class ReportBaseViewModel : ComposedViewModelBase
{
    public ReportBaseViewModel()
    {
        Messenger.Default.Register<BaseCurrencyChanged>(this, _ => { RefreshData().ConfigureAwait(false); });
        Messenger.Default.Register<AccountsChanged>(this, _ => { RefreshData().ConfigureAwait(false); });
    }
        
    public virtual Currency BaseCurrency { get; set; }

    public abstract Task RefreshData();
}