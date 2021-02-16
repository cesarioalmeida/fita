using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace fita.ui.Services
{
    public class GridControlService : ServiceBase, IGridControlService
    {
        protected GridControl ActualControl => this.AssociatedObject as GridControl;

        public void Refresh()
        {
            ActualControl.RefreshData();
        }
    }
}
