﻿using System.Threading.Tasks;
using DevExpress.Mvvm;
using fita.ui.Messages;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Reports
{
    public abstract class ReportBaseViewModel : ComposedViewModelBase
    {
        public ReportBaseViewModel()
        {
            Messenger.Default.Register<BaseCurrencyChanged>(this, _ => { RefreshData(); });
            Messenger.Default.Register<AccountsChanged>(this, _ => { RefreshData(); });
        }

        public abstract Task RefreshData();
    }
}