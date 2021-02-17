using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using fita.data.Models;
using fita.services;
using fita.services.Core;
using fita.services.Repositories;
using fita.ui.ViewModels.HistoricalData;
using fita.ui.Views.Currencies;
using fita.ui.Views.HistoricalData;
using twentySix.Framework.Core.Extensions;
using twentySix.Framework.Core.Messages;
using twentySix.Framework.Core.UI.Enums;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Categories
{
    [POCOViewModel]
    public class CategoriesViewModel : ComposedDocumentViewModelBase
    {
        private bool fireChangeNotification;

        public override object Title { get; set; } = "Categories";

        protected virtual IDocumentManagerService DocumentManagerService => null;

        public void Close()
        {
            if (fireChangeNotification)
            {
            }

            //Data.Clear();
            DocumentOwner?.Close(this);
        }

        public async Task RefreshData()
        {
            IsBusy = true;

            //Data.BeginUpdate();
            //Data.Clear();
        }
        
    }
}