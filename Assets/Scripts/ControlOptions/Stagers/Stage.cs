
using LightControls.Controllers;
using LightControls.ControlOptions.ControlGroups;
using LightControls.ControlOptions.ControlGroups.Data;
using LightControls.Utilities;
using UnityEngine;

namespace LightControls.ControlOptions.Stages
{
    public interface IUnique
    {
        string GUID { get; }
    }

    [System.Serializable]
    public abstract class Stage : IUnique
    {
        public string GUID => guid;

        [SerializeField]
        private string guid;
        
        public StagingOptions ContinueFor = StagingOptions.Iterations;
        public StagingOptions AdvanceAfter = StagingOptions.Iterations;

        public EventDetector ContinueForEventDetector;

        public ListControlGroupData ContinueForListedTimeGeneration;
        public CurveControlGroupData ContinueForCurvedTimeGeneration;

        public int ContinueForIterationsMin = 1;
        public int ContinueForIterationsMax = 1;

        public EventDetector AdvanceAfterEventDetector;

        public ListControlGroupData AdvanceAfterListedTimeGeneration;
        public CurveControlGroupData AdvanceAfterCurvedTimeGeneration;

        public int AdvanceAfterIterationsMin = 1;
        public int AdvanceAfterIterationsMax = 1;

        public bool LimitStarts = false;

        public int MaxStartAmountMin = 1;
        public int MaxStartAmountMax = 1;

        protected Stage()
        {
            ContinueForEventDetector = ContinueForEventDetector ?? new EventDetector();
            ContinueForListedTimeGeneration = ContinueForListedTimeGeneration ?? new ListControlGroupData();
            ContinueForCurvedTimeGeneration = ContinueForCurvedTimeGeneration ?? new CurveControlGroupData();

            AdvanceAfterEventDetector = AdvanceAfterEventDetector ?? new EventDetector();
            AdvanceAfterListedTimeGeneration = AdvanceAfterListedTimeGeneration ?? new ListControlGroupData();
            AdvanceAfterCurvedTimeGeneration = AdvanceAfterCurvedTimeGeneration ?? new CurveControlGroupData();
        }
    }
}