using LiteDB;

namespace fita.data.Models;

public class Currency
{
    public ObjectId CurrencyId { get; set; } = ObjectId.NewObjectId();

    public string Name { get; set; }

    public string Symbol { get; set; }

    public string Culture { get; set; }
}