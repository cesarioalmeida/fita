using LiteDB;
using twentySix.Framework.Core.DTOs;

namespace fita.ui.DTOs
{
    public class AccountDTO : BaseDTO
    {
        public string Name { get; set; }
        
        public ObjectId CurrencyId { get; set; } 
    }
}