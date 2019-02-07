using System;
using System.Collections.Generic;
using System.Linq;
using LightControls.Controllers;
using LightControls.ControlOptions.ControlGroups;
using LightControls.Utilities;
using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    public class InstancedLightControllerStager : InstancedStager, ILightControllerGroup
    {
        protected override Stager stagerData => controllerStager;
        protected override InstancedStage[] stagerStages => controllerStages;

        private LightControllerStager controllerStager;
        private InstancedLightControllerStage[] controllerStages;

        public InstancedLightControllerStager(LightControllerStager stager) 
            : base(stager)
        {
            controllerStager = stager;
            controllerStages = stager.Stages
                .Select(stage => new InstancedLightControllerStage(stage))
                .ToArray();
            
            iterations = new int[controllerStages.Length];
        }

        public void UpdateInstanced()
        {
            if(controllerStager.Stages.Length != controllerStages.Length)
            {
                int previousSize = controllerStages.Length;

                Array.Resize(ref controllerStages, controllerStager.Stages.Length);
                
                for(int i = previousSize; i < controllerStages.Length; i++)
                {
                    controllerStages[i] = new InstancedLightControllerStage(controllerStager.Stages[i]);
                }
            }
        }

        public void ApplyControls()
        {
            foreach (InstancedLightControllerStage stage in activeStages)
            {
                stage.Apply();
            }

            PerformRemoves();
        }
    }
}
