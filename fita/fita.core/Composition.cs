using fita.core.Models;
using LightInject;

namespace fita.core
{
    public class Composition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            //serviceRegistry.Register<Account>();
        }
    }
}