using System;
using LiteDB;
using twentySix.Framework.Core.DTOs;

namespace fita.core.DTOs
{
    public class TransactionDTO : BaseDTO
    {
        public DateTime Date { get; set; }
        
        public string Payee { get; set; }
        
        public string Memo { get; set; }
        
        public string[] Tags { get; set; }
        
        public ObjectId CategoryId { get; set; }
        
        public decimal Value { get; set; }
    }
}