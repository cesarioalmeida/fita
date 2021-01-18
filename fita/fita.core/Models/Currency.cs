using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LiteDB;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class Currency : SynchronizableModel<Currency>, ICloneable
    {
        public Currency()
        {
            Name = "EUR";
            Symbol = "€";
            ExchangeData = new HistoricalData();
        }

        public int CurrencyId { get; set; }

        [Required, MinLength(3)]
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Required]
        public string Symbol
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public HistoricalData ExchangeData { get; set; }

        [BsonIgnore]
        public decimal CurrentExchangeRate => ExchangeData.Data.Count > 0 ? ExchangeData.Data.First().Value : 1m;

        public override bool PropertiesEqual(Currency other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Name) &&
                   other.Symbol.Equals(Symbol) &&
                   other.ExchangeData.Data.SequenceEqual(ExchangeData.Data);
            ;
        }

        public override void SyncFrom(Currency obj)
        {
            Name = (string) obj.Name.Clone();
            Symbol = (string) obj.Symbol.Clone();
            ExchangeData = (HistoricalData) obj.ExchangeData.Clone();
        }

        public object Clone()
        {
            var currency = new Currency();
            currency.SyncFrom(this);
            return currency;
        }
    }
}