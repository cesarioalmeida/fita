using fita.services.External;
using fita.services.Repositories;
using LightInject;

namespace fita.services
{
    public class Composition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<HistoricalDataService>();
            serviceRegistry.Register<CurrencyService>();
            serviceRegistry.Register<ExchangeRateService>();
            serviceRegistry.Register<FileSettingsService>();
            serviceRegistry.Register<IExchangeRateDownloadService, ExchangeRateDownloadService>();
        }
    }
}