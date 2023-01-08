using System;
using fita.data.Enums;
using LiteDB;

namespace fita.data.Models;

public class ClosedPosition
{
    public ObjectId ClosedPositionId { get; set; } = ObjectId.NewObjectId();

    public ObjectId AccountId { get; set; }

    [BsonRef("security")]
    public Security Security { get; set; }
        
    public DateTime? SellDate { get; set; }
        
    public decimal Quantity { get; set; }

    public decimal BuyPrice { get; set; }
        
    public decimal SellPrice { get; set; }
        
    public decimal ProfitLoss { get; set; }
}