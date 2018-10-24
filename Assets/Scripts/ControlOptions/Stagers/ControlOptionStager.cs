using System;
using System.Collections.Generic;
using System.Linq;

using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public class ControlOptionStager : Stager
    {
        public override Stage[] StagerStages => Stages;

        public ControlOptionStage[] Stages;// = new ControlOptionStage[1];

        public ControlOptionStager() : base()
        {
            Stages = Stages ?? new ControlOptionStage[1];
        }
    }
}