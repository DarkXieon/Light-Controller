using System;
using System.Collections.Generic;
using System.Linq;

using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    public abstract class InstancedStager
    {
        public List<InstancedStage> ActiveStages => activeStages.ToList();

        protected abstract Stager stagerData { get; }
        protected abstract InstancedStage[] stagerStages { get; }

        protected List<InstancedStage> activeStages;
        protected List<InstancedStage> waitingForRemove;
        protected int currentStage;
        protected bool waitingForAdvance;
        
        protected int[] iterations;
        private int minIterations;
        
        protected InstancedStager(Stager stager)
        {
            activeStages = new List<InstancedStage>();
            waitingForRemove = new List<InstancedStage>();
            currentStage = -1;
            waitingForAdvance = true;

            minIterations = 0;
        }
        
        public bool UpdateControls()
        {
            if (waitingForAdvance && TryAdvanceStage())
            {
                activeStages.Add(stagerStages[currentStage]);

                waitingForAdvance = false;
            }

            foreach (InstancedStage stage in activeStages)
            {
                bool iterated = stage.Update();

                if (iterated)
                {
                    iterations[Array.IndexOf(stagerStages, stage)]++;
                }

                if (!stage.Continue())
                {
                    Debug.Log("Ending!");

                    waitingForRemove.Add(stage);
                }
            }

            bool finishedIteration = false;

            if (iterations.Min() > minIterations)
            {
                minIterations = iterations.Min();

                finishedIteration = true;
            }

            if (currentStage > -1 && !activeStages.Contains(stagerStages[currentStage]) && stagerStages[currentStage].AdvanceAfter == StagingOptions.Iterations)
            {
                stagerStages[currentStage].Update();
            }

            if (!waitingForAdvance && stagerStages[currentStage].Advance())
            {
                Debug.Log("Advancing!");

                waitingForAdvance = true;
            }

            return finishedIteration;
        }

        public void PerformRemoves()
        {
            //foreach(InstancedStage stage in waitingForRemove)
            //{
            //    iterations[Array.IndexOf(stagerStages, stage)] = 0;
            //}

            activeStages = activeStages.Except(waitingForRemove).ToList();
            waitingForRemove.Clear();
        }

        private bool TryAdvanceStage()
        {
            //timeWaited += Time.deltaTime;

            //if(timeWaited >= timeToWait)
            //{
            int found = GetNextAvailableStage();

            if (found > -1)
            {
                currentStage = found;

                return true;
            }
            //}

            return false;
        }

        private int GetNextAvailableStage()
        {
            int nextIndex = currentStage;
            int foundIndex = -1;

            if (activeStages.Count == stagerStages.Length || (currentStage > -1 && activeStages.Count == stagerStages.Length - 1 && !activeStages.Contains(stagerStages[currentStage]) && !stagerData.CanSkipStages && !stagerData.RandomizeNextStage))
            {
                return -1;
            }

            if (stagerData.RandomizeNextStage)
            {
                List<InstancedStage> randomizedNonActiveStages = RandomizedNonActiveStages().ToList();

                randomizedNonActiveStages.Remove(stagerStages[currentStage]);

                InstancedStage found = randomizedNonActiveStages.FirstOrDefault(stage => stage.Start());

                foundIndex = found != null
                    ? Array.IndexOf(stagerStages, found)
                    : -1;
            }
            else
            {
                bool acceptableMatch = false;

                do
                {
                    nextIndex = Mathf.RoundToInt(Mathf.Repeat(currentStage + 1, stagerStages.Length));

                    acceptableMatch = !activeStages.Contains(stagerStages[nextIndex]) && (nextIndex != currentStage || stagerData.CanSkipStages) && stagerStages[nextIndex].Start();

                    foundIndex = acceptableMatch
                        ? nextIndex
                        : -1;
                }
                while (stagerData.CanSkipStages && (nextIndex != currentStage || !activeStages.Contains(stagerStages[currentStage])) && !acceptableMatch);
            }

            return foundIndex;
        }

        private InstancedStage[] RandomizedNonActiveStages()
        {
            InstancedStage[] nonActive = stagerStages.Except(activeStages).ToArray();
            InstancedStage[] copy = nonActive.ToArray();

            List<int> takenList = new List<int>();

            foreach (InstancedStage stage in copy)
            {
                int randomIndex = UnityEngine.Random.Range(0, nonActive.Length);

                while (takenList.Contains(randomIndex))
                {
                    randomIndex = UnityEngine.Random.Range(0, nonActive.Length);
                }

                nonActive[randomIndex] = stage;
            }

            return nonActive;
        }
    }
}
