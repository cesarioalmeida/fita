using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace fita.data.Models
{
    public class HistoricalData
    {
        public ObjectId HistoricalDataId { get; set; }

        public string Name { get; set; }

        public SortedDictionary<DateTime, HistoricalPoint> Data { get; set; } = new();

        [BsonIgnore]
        public DateTime? LatestDate => Data.FirstOrDefault().Value?.Date;

        [BsonIgnore]
        public decimal? LatestValue => Data.FirstOrDefault().Value?.Value;
    }
}