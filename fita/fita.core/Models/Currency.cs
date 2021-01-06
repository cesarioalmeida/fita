using fita.core.DTOs;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class Currency : SynchronizableModelWithDTO<Currency, CurrencyDTO>
    {
        public Currency()
        {
            Name = "Euro";
            Symbol = "€";
            ExchangeData = new HistoricalData();
        }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public HistoricalData ExchangeData { get; set; }

        public override CurrencyDTO GetDTO()
        {
            return new()
            {
                Id = Id,
                IsDeleted = IsDeleted,
                LastUpdated = LastUpdated,
                Name = Name,
                Symbol = Symbol,
                HistoricalDataId = ExchangeData.Id
            };
        }

        public override bool PropertiesEqual(Currency other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Name)
                   && other.Symbol.Equals(Symbol)
                   && other.ExchangeData.Id == ExchangeData.Id;
        }

        public override void SyncFrom(Currency obj)
        {
            IsDeleted = obj.IsDeleted;
            LastUpdated = obj.LastUpdated;
            Name = (string) obj.Name?.Clone();
            Symbol = (string) obj.Symbol?.Clone();
            ExchangeData = new HistoricalData();
            ExchangeData.AddOrUpdate(obj.ExchangeData.Data);
        }
    }
}