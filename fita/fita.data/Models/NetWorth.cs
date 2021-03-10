using System;
using LiteDB;

namespace fita.data.Models
{
    public class NetWorth
    {
        public ObjectId NetWorthId { get; set; } = ObjectId.NewObjectId();

        public DateTime Date { get; set; }
        
        public decimal Total { get; set; }

        public decimal Banks { get; set; }
        
        public decimal CreditCards { get; set; }
        
        public decimal Investments { get; set; }
        
        public decimal Assets { get; set; }
    }
}