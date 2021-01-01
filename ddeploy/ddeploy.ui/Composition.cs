using ddeploy.ui.Views;
using LightInject;

namespace ddeploy.ui
{
    public class Composition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<MainWindow>(new PerContainerLifetime());
            //serviceRegistry.Register<ShellViewModel>();
        }
    }
}