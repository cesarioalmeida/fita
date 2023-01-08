using DevExpress.Mvvm.UI;
using System.Windows;

namespace fita.ui.Services;

public class FocusService : ServiceBase, IFocusService
{
    protected FrameworkElement ActualControl => AssociatedObject;

    public void Focus() => ActualControl?.Focus();
}