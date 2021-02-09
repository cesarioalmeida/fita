using System;
using LiteDB;

namespace fita.data.Models
{
    public class HistoricalData
    {
        public ObjectId HistoricalDataId { get; set; }
        
        public DateTime Date { get; set; }
        
        public decimal Value { get; set; }
    }
}