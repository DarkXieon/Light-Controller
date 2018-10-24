using System;

using UnityEngine;

using static UnityEngine.ParticleSystem;

namespace LightControls.ControlOptions.ControlGroups.Data
{
    [Serializable]
    public class CurveControlGroupData
    {
        public bool IsCurveBased => MinMaxCurve.mode == ParticleSystemCurveMode.Curve || MinMaxCurve.mode == ParticleSystemCurveMode.TwoCurves;

        public bool UseControl => useControl;
        public MinMaxCurve MinMaxCurve => minMaxCurve;
        public MinMaxTrackerData MaxCurveTracker => maxCurveTracker;
        public MinMaxTrackerData MinCurveTracker => minCurveTracker;
        
        public float MaxValue => maxValue;
        public float MinValue => minValue;

        [SerializeField] private bool useControl;
        [SerializeField] private MinMaxCurve minMaxCurve;
        [SerializeField] private MinMaxTrackerData maxCurveTracker;
        [SerializeField] private MinMaxTrackerData minCurveTracker;

        [SerializeField] private float maxValue;
        [SerializeField] private float minValue;

        public CurveControlGroupData(bool use = false)
        {
            useControl = use;
            minMaxCurve = new MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 10f), AnimationCurve.Linear(0f, 5f, 1f, 10f));
            maxCurveTracker = new MinMaxTrackerData();
            minCurveTracker = new MinMaxTrackerData();
        }
    }
}
