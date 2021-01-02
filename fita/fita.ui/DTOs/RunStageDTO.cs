using System;
using fita.ui.Common;
using twentySix.Framework.Core.DTOs;

namespace fita.ui.DTOs
{
    public class RunStageDTO : BaseDTO
    {
        public string Name { get; set; }

        public RunState State { get; set; }

        public DateTime Started { get; set; }
        
        public DateTime Finished { get; set; }

        public string Logs { get; set; }
    }
}