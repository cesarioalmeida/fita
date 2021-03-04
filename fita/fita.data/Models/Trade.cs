using System;
using fita.data.Enums;
using LiteDB;

namespace fita.data.Models
{
    public class Trade
    {
        public ObjectId TradeId { get; set; } = ObjectId.NewObjectId();

        public ObjectId AccountId { get; set; }

        [BsonRef("security")]
        public Security Security { get; set; }
        
        public DateTime? Date { get; set; }
        
        public TradeActionEnum Action { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }
        
        public decimal Value { get; set; }

        [BsonIgnore]
        public decimal BreakEvenPrice => Quantity > 0m ? Value / Quantity : 0m;
        
        [BsonIgnore]
        public decimal CommissionsTaxes => Value - Quantity * Price;
    }
}