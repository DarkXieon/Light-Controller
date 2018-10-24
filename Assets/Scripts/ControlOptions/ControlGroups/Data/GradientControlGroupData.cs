using UnityEditor;
using UnityEngine;

using MinMaxGradient = UnityEngine.ParticleSystem.MinMaxGradient;

namespace LightControls.ControlOptions.ControlGroups.Data
{
    [System.Serializable]
    public class GradientControlGroupData
    {
        public MinMaxGradient MinMaxGradient => minMaxGradient;
        public MinMaxTrackerData MinMaxTracker => minMaxTracker;

        [SerializeField] private MinMaxGradient minMaxGradient;
        [SerializeField] private MinMaxTrackerData minMaxTracker;

        public GradientControlGroupData()
        {
            minMaxGradient = new MinMaxGradient(new Gradient(), new Gradient());
            minMaxTracker = new MinMaxTrackerData();
        }
    }
}
