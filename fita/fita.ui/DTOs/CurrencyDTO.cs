﻿using LiteDB;
using twentySix.Framework.Core.DTOs;

namespace fita.ui.DTOs
{
    public class CurrencyDTO : BaseDTO
    {
        public string Name { get; set; }
        
        public string Symbol { get; set; }

        public ObjectId HistoricalDataId { get; set; } 
    }
}