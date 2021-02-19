using LiteDB;

namespace fita.data.Models
{
    public class SecurityHistory
    {
        public ObjectId SecurityHistoryId { get; set; } = ObjectId.NewObjectId();

        [BsonRef("security")]
        public Security Security { get; set; }

        [BsonRef("historicaldata")]
        public HistoricalData Price { get; set; }
    }
}