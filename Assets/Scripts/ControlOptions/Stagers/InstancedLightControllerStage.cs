using System;
using System.Linq;

using LightControls.Controllers;

using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    public class InstancedLightControllerStage : InstancedStage
    {
        protected override Stage stageData => lightControllerStage;
        
        private LightControllerStage lightControllerStage;

        private LightController[] clonedLightControllers;

        public InstancedLightControllerStage(LightControllerStage stage) 
            : base(stage)
        {
            lightControllerStage = stage;

            clonedLightControllers = lightControllerStage.LightControllers
                .Where(controller => controller != null)
                .Select(controller =>
                {
                    return controller.gameObject.scene.name == null
                        ? UnityEngine.Object.Instantiate(controller)
                        : controller;
                }).ToArray();

            iterations = new int[clonedLightControllers.Length];

            foreach (LightController lightController in lightControllerStage.LightControllers)
            {
                lightController.IsChild = true;
            }
            foreach (LightController lightController in clonedLightControllers)
            {
                lightController.IsChild = true;
            }
        }
        
        public override bool Update()
        {
            if (iterations.Length != clonedLightControllers.Length)
            {
                Array.Resize(ref iterations, clonedLightControllers.Length);
            }

            for (int i = 0; i < clonedLightControllers.Length; i++)
            {
                bool performedFullIteration = clonedLightControllers[i].ExternalUpdate();

                if (performedFullIteration)
                {
                    iterations[i]++;
                }
            }

            timeActive += Time.deltaTime;

            if (iterations.Length > 0 && iterations.Min() > minIterations)
            {
                minIterations = iterations.Min();

                return true;
            }

            return false;
        }

        public void Apply()
        {
            for (int i = 0; i < clonedLightControllers.Length; i++)
            {
                clonedLightControllers[i].ExternalApply();
            }
        }
    }
}