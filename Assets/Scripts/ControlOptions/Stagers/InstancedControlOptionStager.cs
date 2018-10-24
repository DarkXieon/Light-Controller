using System.Linq;
using LightControls.Controllers;

namespace LightControls.ControlOptions.Stages
{
    public class InstancedControlOptionStager : InstancedStager
    {
        protected override Stager stagerData => controlOption;
        protected override InstancedStage[] stagerStages => instancedStages;
        
        private ControlOptionStager controlOption;
        private InstancedControlOptionStage[] instancedStages;

        public InstancedControlOptionStager(ControlOptionStager option) 
            : base(option)
        {
            controlOption = option;

            instancedStages = controlOption.Stages
                .Select(stage => new InstancedControlOptionStage(stage))
                .ToArray();

            iterations = new int[instancedStages.Length];
        }

        public bool ApplyOn(ApplicationStages stage)
        {
            return instancedStages.Any(instanced => instanced.ApplyOn(stage));
        }

        public void ApplyControl(ControlOptionInfo controlInfo)
        {
            foreach (InstancedControlOptionStage instanced in activeStages)
            {
                instanced.ApplyControl(controlInfo);
            }

            PerformRemoves();
        }
    }
}