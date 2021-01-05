using LiteDB;
using twentySix.Framework.Core.DTOs;

namespace fita.core.DTOs
{
    public class AccountDTO : BaseDTO
    {
        public string Name { get; set; }

        public ObjectId CurrencyId { get; set; }
        
        public bool IsCreditCard { get; set; }
    }
}