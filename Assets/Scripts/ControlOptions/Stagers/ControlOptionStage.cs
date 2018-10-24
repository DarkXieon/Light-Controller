namespace LightControls.ControlOptions.Stages
{
    [System.Serializable]
    public class ControlOptionStage : Stage
    {
        public LightControlOption[] ControlOptions;

        public ControlOptionStage() : base()
        {
            ControlOptions = ControlOptions ?? new LightControlOption[0];
        }
    }
}