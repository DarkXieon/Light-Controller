namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public class LightControllerStager : Stager
    {
        public override Stage[] StagerStages => Stages;

        public LightControllerStage[] Stages = new LightControllerStage[1] { new LightControllerStage() };

        public LightControllerStager() : base()
        {
            Stages = Stages ?? new LightControllerStage[1];
        }
    }
}
