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
        public override Stage[] StagerStages => stages;

        public ControlOptionStage[] Stages => stages;

        [SerializeField] private ControlOptionStage[] stages;

        public ControlOptionStager() : base()
        {
            stages = stages ?? new ControlOptionStage[1];
        }
    }
}