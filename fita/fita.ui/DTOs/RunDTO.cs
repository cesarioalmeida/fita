using System;
using fita.ui.Common;
using LiteDB;
using twentySix.Framework.Core.DTOs;

namespace fita.ui.DTOs
{
    public class RunDTO : BaseDTO
    {
        public string Name { get; set; }
        
        public RunState State { get; set; } 
        
        public DateTime Queued { get; set; }
        
        public DateTime Started { get; set; }
        
        public ObjectId[] StagesIds { get; set; }
    }
}