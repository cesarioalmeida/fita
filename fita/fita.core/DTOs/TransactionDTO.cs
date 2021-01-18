﻿using System;
using LiteDB;

namespace fita.core.DTOs
{
    public class TransactionDTO
    {
        public DateTime Date { get; set; }
        
        public string Payee { get; set; }
        
        public string Memo { get; set; }
        
        public string[] Tags { get; set; }
        
        public ObjectId CategoryId { get; set; }
        
        public decimal Amount { get; set; }
    }
}