using System;
using System.Linq;

using LightControls.Controllers;

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
            controllerStages = stager.StagerStages
                .Select(stage => new InstancedLightControllerStage((LightControllerStage)stage))
                .ToArray();
            
            iterations = new int[controllerStages.Length];
        }

        //public void UpdateInstanced()
        //{
        //    if(controllerStager.stages.Length != controllerStages.Length)
        //    {
        //        int previousSize = controllerStages.Length;

        //        Array.Resize(ref controllerStages, controllerStager.stages.Length);
                
        //        for(int i = previousSize; i < controllerStages.Length; i++)
        //        {
        //            controllerStages[i] = new InstancedLightControllerStage(controllerStager.stages[i]);
        //        }
        //    }
        //}

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
