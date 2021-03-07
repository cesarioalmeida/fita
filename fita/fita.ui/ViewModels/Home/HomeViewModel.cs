using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Home
{
    [POCOViewModel]
    public class HomeViewModel : ComposedViewModelBase
    {
        public virtual BanksViewModel BanksViewModel { get; set; }

        public virtual CreditCardsViewModel CreditCardsViewModel { get; set; }

        public virtual AssetsViewModel AssetsViewModel { get; set; }

        public virtual InvestmentsViewModel InvestmentsViewModel { get; set; }

        public HomeViewModel()
        {
            BanksViewModel = ViewModelSource.Create<BanksViewModel>();
            CreditCardsViewModel = ViewModelSource.Create<CreditCardsViewModel>();
            AssetsViewModel = ViewModelSource.Create<AssetsViewModel>();
            InvestmentsViewModel = ViewModelSource.Create<InvestmentsViewModel>();
        }

    }
}