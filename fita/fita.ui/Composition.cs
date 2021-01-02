using fita.ui.Views;
using LightInject;

namespace fita.ui
{
    public class Composition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<ShellView>(new PerContainerLifetime());
            //serviceRegistry.Register<ShellViewModel>();
        }
    }
}