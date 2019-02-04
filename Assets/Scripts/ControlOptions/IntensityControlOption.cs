using System;

using LightControls.ControlOptions.ControlGroups.Data;

using UnityEngine;

namespace LightControls.ControlOptions
{
    /// <summary>
    /// These define what the intensity control option can change the value of
    /// </summary>
    [Flags]
    public enum IntensityControlTarget
    {
        Nothing = 0,
        LightIntensity = 1,
        LightColorIntensity = 2,
        MaterialColorIntensity = 4,
        LightRange = 8,
        SpotlightAngle = 16,
        Everything = ~0
    }

    /// <summary>
    /// This defines how the output of the intensity control option changes for a control target
    /// </summary>
    [Serializable]
    public class ControlTargetModifier
    {
        public float Multiplier;
        public float Offset;
        public bool RelateInversely;

        public ControlTargetModifier()
        {
            Multiplier = 1f;
            Offset = 0f;
            RelateInversely = false;
        }
    }

    [CreateAssetMenu(menuName = "Light Controller/Intensity Control Option")]
    public class IntensityControlOption : LightControlOption
    {
        public bool UseCustomRateOfChange => CurvedRoC.UseControl || ListedRoC.UseControl;

        public float MaxValue
        {
            get
            {
                return CurvedIntensities.UseControl
                    ? CurvedIntensities.MaxValue
                    : ListedIntensities.MaxValue;
            }
        }

        public float MinValue
        {
            get
            {
                return CurvedIntensities.UseControl
                    ? CurvedIntensities.MinValue
                    : ListedIntensities.MinValue;
            }
        }

        /// <summary>
        /// These are used to generate intensities that are eventually applied to the control group
        /// </summary>
        #region Intensity Generation Variables

        public ListControlGroupData ListedIntensities => listedIntensities;
        public CurveControlGroupData CurvedIntensities => curvedIntensities;

        #endregion

        /// <summary>
        /// These are used to generate the rate of change of the control, whcih determines how quickly intensities will transition from one to another
        /// </summary>
        #region Rate of Change Generation Variables

        public ListControlGroupData ListedRoC => listedRoC;
        public CurveControlGroupData CurvedRoC => curvedRoC;
        public RoCMode RoCMode => roCMode; //Determines the exact method of calculation when intensities are transitioning

        #endregion

        /// <summary>
        /// Used to modify what the control will actually be modifying
        /// </summary>
        #region Light Application Variables

        public IntensityControlTarget ControlTarget => controlTarget;
        public ControlTargetModifier[] IntensityModifers => intensityModifers; //each of these corrospond to a Control Target with a value > 0 in value. The equation for the correct index of a IntensityControlTarget with a value > 0 is
                                                                               //log base two of the enum's int value, going the other way it's the opposite, two to the index power ex. log2(int enum value) = index and 2^(index) = int enum value

        #endregion

        #region Miscellaneous Variables

        public bool NoSmoothTransitions => noSmoothTransitions; //Will make all transitions happen all at once if set to true
        public bool UseLightIntensityFloor => useLightIntensityFloor;
        public bool UseLightIntensityCeiling => useLightIntensityCeiling;
        public float Floor => floor;
        public float Ceiling => ceiling;

        #endregion

        [SerializeField] private ListControlGroupData listedIntensities;
        [SerializeField] private CurveControlGroupData curvedIntensities;

        [SerializeField] private ListControlGroupData listedRoC;
        [SerializeField] private CurveControlGroupData curvedRoC;
        [SerializeField] private RoCMode roCMode = RoCMode.Time;

        [SerializeField] private IntensityControlTarget controlTarget = IntensityControlTarget.Nothing;
        [SerializeField] private ControlTargetModifier[] intensityModifers;

        [SerializeField] private bool noSmoothTransitions;
        [SerializeField] private bool useLightIntensityFloor;
        [SerializeField] private bool useLightIntensityCeiling;
        [SerializeField] private float floor;
        [SerializeField] private float ceiling;
        
        /// <summary>
        /// Initilize our variables here
        /// </summary>
        private void OnEnable()
        {
            listedIntensities = ListedIntensities ?? new ListControlGroupData();
            curvedIntensities = CurvedIntensities ?? new CurveControlGroupData();

            listedRoC = ListedRoC ?? new ListControlGroupData();
            curvedRoC = CurvedRoC ?? new CurveControlGroupData();

            intensityModifers = IntensityModifers ?? new ControlTargetModifier[]
            {
                new ControlTargetModifier(), new ControlTargetModifier(), new ControlTargetModifier(), new ControlTargetModifier(), new ControlTargetModifier()
            };
        }

        public override InstancedControlOption GetInstanced()
        {
            return new InstancedIntensityControlOption(this);
        }
    }
}