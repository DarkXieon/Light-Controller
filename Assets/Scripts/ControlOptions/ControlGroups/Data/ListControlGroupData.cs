using System;

using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups.Data
{
    [Serializable]
    public class ListControlGroupData
    {
        public bool UseControl => useControl;
        public bool RandomizeBetweenValues => randomizeBetweenValues;
        public bool RandomizeChosenEntry => randomizeChosenEntry;
        public float[] ListedValues => listedValues;

        public float MaxValue => maxValue;
        public float MinValue => minValue;

        [SerializeField] private bool useControl;
        [SerializeField] private bool randomizeBetweenValues;
        [SerializeField] private bool randomizeChosenEntry;
        [SerializeField] private float[] listedValues;

        [SerializeField] private float maxValue;
        [SerializeField] private float minValue;

        public ListControlGroupData(bool use = false)
        {
            useControl = use;
            randomizeBetweenValues = false;
            randomizeChosenEntry = false;
            listedValues = new float[1];
        }
    }
}
