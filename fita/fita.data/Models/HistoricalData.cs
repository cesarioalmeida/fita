using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace fita.data.Models
{
    public class HistoricalData
    {
        public ObjectId HistoricalDataId { get; set; } = ObjectId.NewObjectId();

        public string Name { get; set; }

        public Dictionary<DateTime, HistoricalPoint> Data { get; set; } = new();

        [BsonIgnore]
        public DateTime? LatestDate => Data.Any() ? Data.OrderByDescending(x => x.Key).First().Key : null;

        [BsonIgnore]
        public decimal? LatestValue => Data.Any() ? Data.OrderByDescending(x => x.Key).First().Value.Value : null;
    }
}