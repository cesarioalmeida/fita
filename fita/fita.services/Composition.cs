using fita.services.Core;
using fita.services.Repositories;
using LightInject;

namespace fita.services
{
    public class Composition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<HistoricalDataRepoService>();
            serviceRegistry.Register<CurrencyRepoService>();
            serviceRegistry.Register<ExchangeRateRepoService>();
            serviceRegistry.Register<FileSettingsRepoService>();
            serviceRegistry.Register<CategoryRepoService>();
            serviceRegistry.Register<SecurityRepoService>();
            serviceRegistry.Register<SecurityHistoryRepoService>();
            serviceRegistry.Register<AccountRepoService>();

            serviceRegistry.Register<IExchangeRateService, ExchangeRateService>();
            serviceRegistry.Register<ISecurityService, SecurityService>();
        }
    }
}