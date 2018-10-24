using System;
using LightControls.ControlOptions.ControlGroups;
using LightControls.ControlOptions.ControlGroups.Data;
using UnityEngine;

namespace LightControls.ControlOptions
{
    /// <summary>
    /// These define what the color control option can change the color of
    /// </summary>
    [Flags]
    public enum ColorControlTarget
    {
        Light = 1,
        Material = 2,
        Both = ~0,
        Neither = 0
    }

    /// <summary>
    /// This class handles the color assignment of lights and emissive materials
    /// </summary>
    [CreateAssetMenu(menuName = "Light Controller/Color Control Option")]
    public class ColorControlOption : LightControlOption
    {
        /// <summary>
        /// These variables are used in determining the color that this control option will use
        /// </summary>
        #region Color Generation Variables

        public GradientControlGroupData ColorControl; //This gradient control handles tracking the color of the gradient

        #endregion

        #region Rate of Change Generation Variables

        public ListControlGroupData ListedRoC;
        public CurveControlGroupData CurvedRoC;
        public RoCMode RoCMode = RoCMode.Time;

        #endregion

        #region Color Application Variables

        public ColorControlTarget ColorTarget = ColorControlTarget.Neither;

        #endregion

        private void OnEnable()
        {
            ColorControl = ColorControl ?? new GradientControlGroupData();
            ListedRoC = ListedRoC ?? new ListControlGroupData();
            CurvedRoC = CurvedRoC ?? new CurveControlGroupData();
        }

        public override InstancedControlOption GetInstanced()
        {
            return new InstancedColorControlOption(this);
        }
    }
}