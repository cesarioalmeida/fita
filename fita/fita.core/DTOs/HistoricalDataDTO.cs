using System;
using System.Collections.Generic;
using twentySix.Framework.Core.DTOs;

namespace fita.core.DTOs
{
    public class HistoricalDataDTO : BaseDTO
    {
        public Dictionary<DateTime, decimal> Data { get; set; } 
    }
}