using System;

using LightControls.ControlOptions;
using UnityEngine;

namespace LightControls.Controllers.Data
{
    [Serializable]
    public class LightControllerGroupData
    {
        public ControlOptionGroupData ControlOptionGroupData => controlOptionGroupData;
        public LightControlOption[] LightControlOptions => lightControlOptions;

        [SerializeField] private ControlOptionGroupData controlOptionGroupData;
        [SerializeField] private LightControlOption[] lightControlOptions = new LightControlOption[0];

        public LightControllerGroupData()
        {
            controlOptionGroupData = new ControlOptionGroupData();

            lightControlOptions = new LightControlOption[0];
        }
    }
}
