using System;
using System.Linq;
using LightControls.Controllers;
using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    public class InstancedControlOptionStage : InstancedStage
    {
        protected override Stage stageData => controlStage;
        
        private ControlOptionStage controlStage;

        private InstancedControlOption[] controlOptions; //this is gotten from the create instance method which will be performed on all of the controlStage.controlOptions refences

        public InstancedControlOptionStage(ControlOptionStage stage) 
            : base(stage)
        {
            controlStage = stage;

            controlOptions = controlStage.ControlOptions
                .Select(option => option.GetInstanced())
                .ToArray();

            iterations = new int[controlOptions.Length];
        }
        
        public bool ApplyOn(ApplicationStages stage)
        {
            return controlOptions.Any(element => element.ApplyOn(stage));
        }

        public override bool Update()
        {
            if (iterations.Length != controlOptions.Length)
            {
                Array.Resize(ref iterations, controlOptions.Length);
            }

            for (int i = 0; i < controlOptions.Length; i++)
            {
                //if (controlOptions[i].UseControl)
                //{
                bool performedFullIteration = controlOptions[i].UpdateControl();

                if (performedFullIteration)
                {
                    iterations[i]++;
                }
                //}
            }

            timeActive += Time.deltaTime;

            if (iterations.Min() > minIterations)
            {
                minIterations = iterations.Min();

                return true;
            }

            return false;
        }

        public void ApplyControl(ControlOptionGroup controlInfo)
        {
            for (int i = 0; i < controlOptions.Length; i++)
            {
                //if (controlOptions[i].UseControl)
                //{
                controlOptions[i].ApplyControl(controlInfo);
                //}
            }
        }

        //public override void Initilize()
        //{
        //    timesStarted = 0;

        //    maxStartAmount = UnityEngine.Random.Range(MaxStartAmountMin, MaxStartAmountMax);

        //    foreach (InstancedControlOption controlOption in controlOptions)
        //    {
        //        controlOption.ClonedOption.OnEnable();
        //    }
        //}
    }
}
