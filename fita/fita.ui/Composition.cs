using fita.ui.Views;
using LightInject;
using twentySix.Framework.Core.UI.Interfaces;

namespace fita.ui
{
    public class Composition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<ShellView>();
            serviceRegistry.Register<IIsModelView, CurrenciesView>(nameof(CurrenciesView) + "Service");
            serviceRegistry.Register<IIsModelView, CategoriesView>(nameof(CategoriesView) + "Service");
        }
    }
}