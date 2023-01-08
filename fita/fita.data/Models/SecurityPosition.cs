using LiteDB;

namespace fita.data.Models;

public class SecurityPosition
{
    public ObjectId SecurityPositionId { get; set; } = ObjectId.NewObjectId();
        
    public ObjectId AccountId { get; set; }
        
    [BsonRef("security")]
    public Security Security { get; set; }

    public decimal Quantity { get; set; }

    public decimal Value { get; set; }

    [BsonIgnore] 
    public decimal BreakEvenPrice => Quantity > 0m ? Value / Quantity : 0m;
}