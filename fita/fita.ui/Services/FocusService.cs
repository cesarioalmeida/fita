using DevExpress.Mvvm.UI;
using System.Windows;

namespace fita.ui.Services
{
    public class FocusService : ServiceBase, IFocusService
    {
        protected FrameworkElement ActualControl => this.AssociatedObject;

        public void Focus()
        {
            this.ActualControl?.Focus();
        }
    }
}
