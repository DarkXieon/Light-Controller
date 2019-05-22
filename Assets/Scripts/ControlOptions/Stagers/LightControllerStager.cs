using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public class LightControllerStager : Stager
    {
        public override Stage[] StagerStages => stages;

        [SerializeField] private LightControllerStage[] stages = new LightControllerStage[1] { new LightControllerStage() };

        public LightControllerStager() : base()
        {
            stages = stages ?? new LightControllerStage[1] { new LightControllerStage() };
        }
    }
}
