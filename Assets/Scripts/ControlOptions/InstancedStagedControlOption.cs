using LightControls.Controllers;
using LightControls.ControlOptions.Stages;

namespace LightControls.ControlOptions
{
    public class InstancedStagedControlOption : InstancedControlOption
    {
        private StagedControlOption stagedControlOption;

        private InstancedControlOptionStager instancedOptionStager;

        public InstancedStagedControlOption(StagedControlOption option)
        {
            stagedControlOption = option;

            instancedOptionStager = new InstancedControlOptionStager(option.Stager);
        }
        
        public override bool ApplyOn(ApplicationStages stage)
        {
            return instancedOptionStager.ApplyOn(stage);
        }

        public override bool UpdateControl()
        {
            return instancedOptionStager.UpdateControls();
        }

        public override void ApplyControl(ControlOptionInfo controlInfo)
        {
            instancedOptionStager.ApplyControl(controlInfo);
        }
    }
}