using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using fita.data.Models;
using fita.services.Repositories;

namespace fita.ui.ViewModels.Reports
{
    [POCOViewModel]
    public class NetWorthReportViewModel : ReportBaseViewModel
    {
        public LockableCollection<Model> Data { get; set; } = new();

        public virtual Currency BaseCurrency { get; set; }
        
        public NetWorthRepoService NetWorthRepoService { get; set; }

        public override async Task RefreshData()
        {
            IsBusy = true;
            Data.BeginUpdate();

            try
            {
                Data.Clear();

                var netWorth = (await NetWorthRepoService.AllAsync()).OrderBy(x => x.Date);

                foreach (var item in netWorth)
                {
                    Data.Add(new Model(item));
                }

            }
            finally
            {
                Data.EndUpdate();
                IsBusy = false;
            }
        }

        public class Model
        {
            public Model(NetWorth item)
            {
                Date = item.Date;
                NetWorth = item.Total;
                Banks = item.Banks;
                Investments = item.Investments;
                CreditCards = item.CreditCards;
                Assets = item.Assets;
            }

            public DateTime Date { get; }

            public decimal NetWorth { get; }

            public decimal Banks { get; }

            public decimal Investments { get; }

            public decimal CreditCards { get; }

            public decimal Assets { get; }
        }
    }
}