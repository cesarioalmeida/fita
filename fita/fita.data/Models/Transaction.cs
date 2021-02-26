using System;
using LiteDB;

namespace fita.data.Models
{
    public class Transaction
    {
        public ObjectId TransactionId { get; set; } = ObjectId.NewObjectId();

        public ObjectId AccountId { get; set; }

        public DateTime? Date { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }

        [BsonRef("category")]
        public Category Category { get; set; }
        
        public ObjectId AssociatedTransactionId { get; set; }

        public decimal? Payment { get; set; }

        public decimal? Deposit { get; set; }

        public override string ToString()
        {
            return $"[{Date?.ToShortDateString()}] {Description}";
        }
    }
}