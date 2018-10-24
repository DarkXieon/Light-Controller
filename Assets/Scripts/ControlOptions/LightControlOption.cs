using UnityEngine;

namespace LightControls.ControlOptions
{
    /// <summary>
    /// This will be used by some control options to determine how the rate of change for the control will be calculated
    /// </summary>
    public enum RoCMode
    {
        Value,
        Time
    }

    /// <summary>
    /// For each element in this enumeration, a seperate apply control call will be made in the order of the element values
    /// </summary>
    public enum ApplicationStages
    {
        IntensityApplication = 1,
        LightColorApplication = 2,
        MaterialColorApplication = 4,
        ColorIntensityApplication = 8,
        OtherApplications = 16
    }
    
    /// <summary>
    /// This is the base class for all light control options
    /// </summary>
    public abstract class LightControlOption : ScriptableObject
    {
        public abstract InstancedControlOption GetInstanced();
    }
}