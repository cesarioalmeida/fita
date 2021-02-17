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

        public List<HistoricalDataPoint> DataPoints { get; set; } = new();

        [BsonIgnore]
        public DateTime? LatestDate => DataPoints.Any() ? DataPoints.OrderByDescending(x => x.Date).First().Date : null;

        [BsonIgnore]
        public decimal? LatestValue => DataPoints.Any() ? DataPoints.OrderByDescending(x => x.Date).First().Value : null;
    }
}