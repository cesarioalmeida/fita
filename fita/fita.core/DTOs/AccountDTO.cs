using LiteDB;

namespace fita.core.DTOs
{
    public class AccountDTO
    {
        public string Group { get; set; }
        
        public string Name { get; set; }

        public ObjectId CurrencyId { get; set; }

        public bool IsCreditCard { get; set; }

        public ObjectId[] TransactionsIds { get; set; }
    }
}