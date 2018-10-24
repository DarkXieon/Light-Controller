using System;
using System.Collections.Generic;
using System.Linq;

using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public abstract class Stager //maybe rename this stager data
    {
        public abstract Stage[] StagerStages { get; }
        
        public bool RandomizeNextStage;
        public bool CanSkipStages;
    }
}
