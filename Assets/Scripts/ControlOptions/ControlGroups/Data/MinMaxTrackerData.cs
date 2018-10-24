using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups.Data
{
    [System.Serializable]
    public class MinMaxTrackerData
    {
        public float[] MinMaxPoints => minMaxPoints; //It assumed that this is sorted in ascending order
        public MinMaxWrapMode WrapMode => wrapMode;

        [SerializeField] private float[] minMaxPoints;
        [SerializeField] private MinMaxWrapMode wrapMode;

        public MinMaxTrackerData()
        {
            minMaxPoints = new float[0];
            wrapMode = MinMaxWrapMode.Clamp;
        }
    }
}
