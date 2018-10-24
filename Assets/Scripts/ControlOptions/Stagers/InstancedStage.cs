using System;
using System.Collections.Generic;
using System.Linq;
using LightControls.Controllers;
using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    public abstract class InstancedStage
    {
        public StagingOptions ContinueFor => stageData.ContinueFor;
        public StagingOptions AdvanceAfter => stageData.AdvanceAfter;
        
        protected abstract Stage stageData { get; }

        protected int[] iterations;
        protected int minIterations; //This is the lowest amount of iterations out of all controls that has been performed

        protected int timesStarted = 0;
        protected float timeActive = 0f;

        protected int maxStartAmount = 0; //the largest allowable amount of times that the start method can be called

        protected float continueForTime = 0f;
        protected int continueForIterations = 0;

        protected float advanceAfterTime = 0f;
        protected int advanceAfterIterations = 0;

        //initalize these in the initilize method
        protected ListControlGroup continueForListedTimeGeneration;
        protected CurveControlGroup continueForCurvedTimeGeneration;

        protected ListControlGroup advanceAfterListedTimeGeneration;
        protected CurveControlGroup advanceAfterCurvedTimeGeneration;
        
        public InstancedStage(Stage data)
        {
            continueForListedTimeGeneration = new ListControlGroup(data.ContinueForListedTimeGeneration);
            continueForCurvedTimeGeneration = new CurveControlGroup(data.ContinueForCurvedTimeGeneration);

            advanceAfterListedTimeGeneration = new ListControlGroup(data.AdvanceAfterListedTimeGeneration);
            advanceAfterCurvedTimeGeneration = new CurveControlGroup(data.AdvanceAfterCurvedTimeGeneration);

            maxStartAmount = UnityEngine.Random.Range(data.MaxStartAmountMin, data.MaxStartAmountMax);
            timesStarted = 0;

            minIterations = 0;
            timeActive = 0f;

            continueForTime = 0f;
            continueForIterations = 0;

            advanceAfterTime = 0f;
            advanceAfterIterations = 0;
        }
        
        public abstract bool Update();
        
        public bool Continue()
        {
            switch (ContinueFor)
            {
                case StagingOptions.Iterations:
                    return minIterations < continueForIterations;
                case StagingOptions.Time:
                    return timeActive < continueForTime;
                case StagingOptions.Event:
                    return stageData.ContinueForEventDetector.EventDetected(new StageEventInfo(false));
                default:
                    return true;
            }
        }

        public bool Advance()
        {
            switch (AdvanceAfter)
            {
                case StagingOptions.Iterations:
                    return minIterations >= advanceAfterIterations;
                case StagingOptions.Time:
                    return timeActive >= advanceAfterTime;
                case StagingOptions.Event:
                    return stageData.AdvanceAfterEventDetector.EventDetected(new StageEventInfo(!Continue()));
                default:
                    return false;
            }
        }

        public bool Start()
        {
            if (!stageData.LimitStarts || timesStarted < maxStartAmount)
            {
                timesStarted++;

                if (continueForListedTimeGeneration.UseControl)
                    continueForListedTimeGeneration.IncrementControlValue();

                if (continueForCurvedTimeGeneration.UseControl)
                    continueForCurvedTimeGeneration.IncrementControlValue();

                if (advanceAfterListedTimeGeneration.UseControl)
                    advanceAfterListedTimeGeneration.IncrementControlValue();

                if (advanceAfterCurvedTimeGeneration.UseControl)
                    advanceAfterCurvedTimeGeneration.IncrementControlValue();

                continueForTime = continueForListedTimeGeneration.UseControl
                    ? continueForListedTimeGeneration.GetControlValue()
                    : continueForCurvedTimeGeneration.GetControlValue();

                continueForIterations = UnityEngine.Random.Range(stageData.ContinueForIterationsMin, stageData.ContinueForIterationsMax);

                advanceAfterTime = advanceAfterListedTimeGeneration.UseControl
                    ? advanceAfterListedTimeGeneration.GetControlValue()
                    : advanceAfterCurvedTimeGeneration.GetControlValue();

                advanceAfterIterations = UnityEngine.Random.Range(stageData.AdvanceAfterIterationsMin, stageData.AdvanceAfterIterationsMax);

                timeActive = 0f;

                iterations = new int[iterations.Length];

                minIterations = 0;

                Debug.Log("Started!");

                return true;
            }

            return false;
        }
    }
}