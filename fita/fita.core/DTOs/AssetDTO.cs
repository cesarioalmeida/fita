using LiteDB;
using twentySix.Framework.Core.DTOs;

namespace fita.core.DTOs
{
    public class AssetDTO : BaseDTO
    {
        public string Name { get; set; }
        
        public string Symbol { get; set; }
        
        public ObjectId CurrencyId { get; set; }

        public ObjectId HistoricalDataId { get; set; } 
    }
}