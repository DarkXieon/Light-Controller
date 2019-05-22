
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

        public StagingOptions ContinueFor => continueFor;
        public StagingOptions AdvanceAfter => advanceAfter;

        public EventDetector ContinueForEventDetector => continueForEventDetector;

        public ListControlGroupData ContinueForListedTimeGeneration => continueForListedTimeGeneration;
        public CurveControlGroupData ContinueForCurvedTimeGeneration => continueForCurvedTimeGeneration;

        public int ContinueForIterationsMin => continueForIterationsMin;
        public int ContinueForIterationsMax => continueForIterationsMax;

        public EventDetector AdvanceAfterEventDetector => advanceAfterEventDetector;

        public ListControlGroupData AdvanceAfterListedTimeGeneration => advanceAfterListedTimeGeneration;
        public CurveControlGroupData AdvanceAfterCurvedTimeGeneration => advanceAfterCurvedTimeGeneration;

        public int AdvanceAfterIterationsMin => advanceAfterIterationsMin;
        public int AdvanceAfterIterationsMax => advanceAfterIterationsMax;

        public bool LimitStarts => limitStarts;

        public int MaxStartAmountMin => maxStartAmountMin;
        public int MaxStartAmountMax => maxStartAmountMax;

        [SerializeField] private string guid;

        [SerializeField] private StagingOptions continueFor = StagingOptions.Iterations;
        [SerializeField] private StagingOptions advanceAfter = StagingOptions.Iterations;

        [SerializeField] private EventDetector continueForEventDetector;

        [SerializeField] private ListControlGroupData continueForListedTimeGeneration;
        [SerializeField] private CurveControlGroupData continueForCurvedTimeGeneration;

        [SerializeField] private int continueForIterationsMin = 1;
        [SerializeField] private int continueForIterationsMax = 1;

        [SerializeField] private EventDetector advanceAfterEventDetector;

        [SerializeField] private ListControlGroupData advanceAfterListedTimeGeneration;
        [SerializeField] private CurveControlGroupData advanceAfterCurvedTimeGeneration;

        [SerializeField] private int advanceAfterIterationsMin = 1;
        [SerializeField] private int advanceAfterIterationsMax = 1;

        [SerializeField] private bool limitStarts = false;

        [SerializeField] private int maxStartAmountMin = 1;
        [SerializeField] private int maxStartAmountMax = 1;

        protected Stage()
        {
            continueForEventDetector = continueForEventDetector ?? new EventDetector();
            continueForListedTimeGeneration = continueForListedTimeGeneration ?? new ListControlGroupData();
            continueForCurvedTimeGeneration = continueForCurvedTimeGeneration ?? new CurveControlGroupData();

            advanceAfterEventDetector = advanceAfterEventDetector ?? new EventDetector();
            advanceAfterListedTimeGeneration = advanceAfterListedTimeGeneration ?? new ListControlGroupData();
            advanceAfterCurvedTimeGeneration = advanceAfterCurvedTimeGeneration ?? new CurveControlGroupData();
        }
    }
}