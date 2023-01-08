using LiteDB;

namespace fita.data.Models;

public class ExchangeRate
{
    public ObjectId ExchangeRateId { get; set; } = ObjectId.NewObjectId();

    [BsonRef("currency")]
    public Currency FromCurrency { get; set; }

    [BsonRef("currency")]
    public Currency ToCurrency { get; set; }

    [BsonRef("historicaldata")]
    public HistoricalData Rate { get; set; }
}