using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public class ControlOptionStage : Stage
    {
        public LightControlOption[] ControlOptions => controlOptions;

        [SerializeField] private LightControlOption[] controlOptions;

        public ControlOptionStage() : base()
        {
            controlOptions = controlOptions ?? new LightControlOption[0];
        }
    }
}