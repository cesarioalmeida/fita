using System.Collections.Generic;
using LiteDB;

namespace fita.data.Models
{
    public class ExchangeRate
    {
        public ObjectId ExchangeRateId { get; set; }

        [BsonRef("currency")]
        public Currency FromCurrency { get; set; }

        [BsonRef("currency")]
        public Currency ToCurrency { get; set; }

        [BsonRef("historicaldata")]
        public IList<HistoricalData> HistoricalData { get; set; } = new List<HistoricalData>();
    }
}