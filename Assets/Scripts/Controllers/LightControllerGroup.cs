using System;
using System.Linq;
using LightControls.Controllers.Data;
using LightControls.ControlOptions;

namespace LightControls.Controllers
{
    public class LightControllerGroup : ILightControllerGroup
    {
        private LightControllerGroupData controllerGroupData;

        private ControlOptionGroup controlOptionGroup;
        private InstancedControlOption[] controlOptions;

        private int[] controlIterations = new int[0];
        private int minIterations;

        private ApplicationStages[] applicationStages;

        public LightControllerGroup(LightControllerGroupData info)
        {
            controllerGroupData = info;
            controlOptions = info.LightControlOptions
                .Where(option => option != null)
                .Select(option => option.GetInstanced())
                .ToArray();

            controlOptionGroup = new ControlOptionGroup(info.ControlOptionGroupData, controlOptions);

            applicationStages = ((ApplicationStages[])Enum.GetValues(typeof(ApplicationStages)))
                .OrderBy(stage => (int)stage)
                //.Select(stage => (ApplicationStages)stage)
                .ToArray();

            controlIterations = new int[controlOptions.Length];
            minIterations = 0;
        }
        
        public bool UpdateControls()
        {
            if(controlOptions.Length > 0)
            {
                if (controlIterations.Length != controlOptions.Length)
                {
                    Array.Resize(ref controlIterations, controlOptions.Length);
                }

                for (int i = 0; i < controlOptions.Length; i++)
                {
                    if (controlOptions[i].UpdateControl())
                        controlIterations[i]++;
                }

                if (controlIterations.Min() > minIterations)
                {
                    minIterations = controlIterations.Min();

                    return true;
                }
            }

            return false;
        }

        public void ApplyControls()
        {
            if (controlOptions.Length > 0)
            {
                for (int i = 0; i < applicationStages.Length; i++)
                {
                    controlOptionGroup.CurrentStage = applicationStages[i];

                    for (int k = 0; k < controlOptions.Length; k++)
                    {
                        controlOptions[k].ApplyControl(controlOptionGroup);
                    }
                }
            }
        }
    }
}
