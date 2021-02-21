using fita.data.Enums;
using LiteDB;

namespace fita.data.Models
{
    public class Account
    {
        public ObjectId AccountId { get; set; } = ObjectId.NewObjectId();

        public string Name { get; set; }

        public string BankName { get; set; }

        public AccountTypeEnum Type { get; set; }

        [BsonRef("currency")]
        public Currency Currency { get; set; }

        public decimal InitialBalance { get; set; }
    }
}