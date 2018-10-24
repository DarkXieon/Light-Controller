using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightControls.ControlOptions;
using UnityEngine;

namespace LightControls.Controllers
{
    public class InstancedLightControllerInfo : ILightControllerInfo
    {
        private LightControlInfo controllerInfo;
        private InstancedControlOption[] controlOptions;

        private int[] controlIterations = new int[0];
        private int minIterations;

        private ApplicationStages[] applicationStages;

        public InstancedLightControllerInfo(LightControlInfo info)
        {
            controllerInfo = info;
            controlOptions = info.LightControlOptions
                .Where(option => option != null)
                .Select(option => option.GetInstanced())
                .ToArray();

            applicationStages = ((ApplicationStages[])Enum.GetValues(typeof(ApplicationStages))).OrderBy(stage => (int)stage).Select(stage => (ApplicationStages)stage).ToArray();
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
                    controllerInfo.ControlOptionInfo.CurrentStage = applicationStages[i];

                    controllerInfo.ControlOptionInfo.SaveLightColor = !controlOptions.Any(option => option.ApplyOn(ApplicationStages.LightColorApplication));

                    controllerInfo.ControlOptionInfo.SaveMaterialColor = !controlOptions.Any(option => option.ApplyOn(ApplicationStages.MaterialColorApplication));
                        
                    for (int k = 0; k < controlOptions.Length; k++)
                    {
                        controlOptions[k].ApplyControl(controllerInfo.ControlOptionInfo);
                    }
                }
            }
        }
    }
}
