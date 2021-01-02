using System;
using fita.ui.Common;
using fita.ui.DTOs;
using twentySix.Framework.Core.UI.Models;

namespace ddeploy.ui.Models
{
    public class RunStage : SynchronizableModelWithDTO<RunStage, RunStageDTO>
    {
        public string Name { get; set; }
        
        public RunState State { get; set; }

        public DateTime Started { get; set; }
        
        public DateTime Finished { get; set; }

        public string Logs { get; set; }
        
        public override RunStageDTO GetDTO()
        {
            return new RunStageDTO
            {
                Id = Id,
                Name = Name,
                State = State,
                Started = Started,
                Finished = Finished,
                Logs = Logs
            };
        }

        public override bool PropertiesEqual(RunStage other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Name)
                   && other.State.Equals(State)
                   && other.Started == Started
                   && other.Finished == Finished
                   && other.Logs.Equals(Logs);
        }

        public override void SyncFrom(RunStage obj)
        {
            Name = (string)obj.Name?.Clone();
            State = obj.State;
            Started = obj.Started;
            Finished = obj.Finished;
            Logs = (string)obj.Logs?.Clone();
        }
    }
}