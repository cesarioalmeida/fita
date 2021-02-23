using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using twentySix.Framework.Core.UI.ViewModels;

namespace fita.ui.ViewModels.Home
{
    [POCOViewModel]
    public class HomeViewModel : ComposedViewModelBase
    {
        public async Task RefreshData()
        {
            IsBusy = true;

            try
            {
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}