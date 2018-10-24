using LightControls.Controllers;

namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public class LightControllerStage : Stage
    {
        public LightController[] LightControllers;

        public LightControllerStage() : base()
        {
            LightControllers = LightControllers ?? new LightController[0];
        }
    }
}