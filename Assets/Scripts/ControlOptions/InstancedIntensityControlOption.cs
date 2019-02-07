using LightControls.Controllers;
using LightControls.ControlOptions.ControlGroups;
using LightControls.Utilities;

using UnityEngine;

namespace LightControls.ControlOptions
{
    public class InstancedIntensityControlOption : InstancedControlOption
    {
        private IntensityControlOption intensityControlOption;

        /// <summary>
        /// These are used to generate intensities that are eventually applied to the control group
        /// </summary>
        #region Intensity Generation Variables

        private ListControlGroup listedIntensities;
        private CurveControlGroup curvedIntensities;

        #endregion

        /// <summary>
        /// These are used to generate the rate of change of the control, whcih determines how quickly intensities will transition from one to another
        /// </summary>
        #region Rate of Change Generation Variables

        private ListControlGroup listedRoC;
        private CurveControlGroup curvedRoC;

        #endregion
        
        private float goalIntensity;
        private float currentIntensity;
        private float currentRateOfChange;
        private float totalChange;
        private bool iterationFinished;

        public InstancedIntensityControlOption(IntensityControlOption controlOption)
        {
            intensityControlOption = controlOption;

            listedIntensities = new ListControlGroup(intensityControlOption.ListedIntensities);
            curvedIntensities = new CurveControlGroup(intensityControlOption.CurvedIntensities);

            listedRoC = new ListControlGroup(intensityControlOption.ListedRoC);
            curvedRoC = new CurveControlGroup(intensityControlOption.CurvedRoC);
            
            currentRateOfChange = 0f;
            currentIntensity = intensityControlOption.CurvedIntensities.UseControl
                ? curvedIntensities.GetControlValue()
                : listedRoC.GetControlValue();
            goalIntensity = GetNextIntensityGoal();
            totalChange = goalIntensity - currentIntensity;
            iterationFinished = false;
        }
        
        public override bool UpdateControl()
        {
            bool previousValue = iterationFinished;

            UpdateIntensityGoal();
            UpdateRateOfChange();
            UpdateCurrentIntensity();
            
            return curvedIntensities.UseControl && curvedIntensities.CurveMode == ParticleSystemCurveMode.Curve && intensityControlOption.RoCMode == RoCMode.Time //under these conditions, the iterations finished variable will be set one update early
                ? previousValue
                : iterationFinished;
        }

        public override bool ApplyOn(ApplicationStages stage)
        {
            return ApplyOn(stage, intensityControlOption.ControlTarget);
        }
        
        public static bool ApplyOn(ApplicationStages stage, IntensityControlTarget controlTarget)
        {
            if (stage == ApplicationStages.IntensityApplication)
            {
                return controlTarget.HasFlag(IntensityControlTarget.LightIntensity);
            }
            if (stage == ApplicationStages.ColorIntensityApplication)
            {
                return controlTarget.HasFlag(IntensityControlTarget.LightColorIntensity) || controlTarget.HasFlag(IntensityControlTarget.MaterialColorIntensity);
            }
            if (stage == ApplicationStages.OtherApplications)
            {
                return controlTarget.HasFlag(IntensityControlTarget.LightRange) || controlTarget.HasFlag(IntensityControlTarget.SpotlightAngle);
            }

            return false;
        }

        public override void ApplyControl(ControlOptionGroup controlOptionInfo)
        {
            float valueMin;
            float valueMax;

            GetMinMax(controlOptionInfo, out valueMin, out valueMax);

            ApplyControl(
                intensity: currentIntensity,
                intensityFloor: valueMin,
                intensityCeiling: valueMax,
                currentTarget: intensityControlOption.ControlTarget,
                modifiers: intensityControlOption.IntensityModifers,
                controlOptionInfo: controlOptionInfo);
        }

        public static void ApplyControl(
            float intensity,
            float intensityFloor,
            float intensityCeiling,
            IntensityControlTarget currentTarget,
            ControlTargetModifier[] modifiers,
            ControlOptionGroup controlOptionInfo)
        {
            if (controlOptionInfo.CurrentStage == ApplicationStages.IntensityApplication && currentTarget.HasFlag(IntensityControlTarget.LightIntensity))
            {
                SetLightValue(
                    valueType: IntensityControlTarget.LightIntensity,
                    intensity: intensity,
                    intensityFloor: intensityFloor,
                    intensityCeiling: intensityCeiling,
                    controlTargets: currentTarget,
                    modifiers: modifiers,
                    controlOptionInfo: controlOptionInfo);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.ColorIntensityApplication && currentTarget.HasFlag(IntensityControlTarget.LightColorIntensity))
            {
                SetLightValue(
                    valueType: IntensityControlTarget.LightColorIntensity,
                    intensity: intensity,
                    intensityFloor: intensityFloor,
                    intensityCeiling: intensityCeiling,
                    controlTargets: currentTarget,
                    modifiers: modifiers,
                    controlOptionInfo: controlOptionInfo);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.ColorIntensityApplication && currentTarget.HasFlag(IntensityControlTarget.MaterialColorIntensity))
            {
                SetLightValue(
                    valueType: IntensityControlTarget.MaterialColorIntensity,
                    intensity: intensity,
                    intensityFloor: intensityFloor,
                    intensityCeiling: intensityCeiling,
                    controlTargets: currentTarget,
                    modifiers: modifiers,
                    controlOptionInfo: controlOptionInfo);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.OtherApplications && currentTarget.HasFlag(IntensityControlTarget.LightRange))
            {
                SetLightValue(
                    valueType: IntensityControlTarget.LightRange,
                    intensity: intensity,
                    intensityFloor: intensityFloor,
                    intensityCeiling: intensityCeiling,
                    controlTargets: currentTarget,
                    modifiers: modifiers,
                    controlOptionInfo: controlOptionInfo);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.OtherApplications && currentTarget.HasFlag(IntensityControlTarget.SpotlightAngle))
            {
                SetLightValue(
                    valueType: IntensityControlTarget.SpotlightAngle,
                    intensity: intensity,
                    intensityFloor: intensityFloor,
                    intensityCeiling: intensityCeiling,
                    controlTargets: currentTarget,
                    modifiers: modifiers,
                    controlOptionInfo: controlOptionInfo);
            }
        }

        public bool UseCustomRateOfChange()
        {
            return intensityControlOption.UseCustomRateOfChange;
        }
        
        /// <summary>
        /// Updates the currentRateOfChange variable which is used to update the currentIntenisty variable
        /// </summary>
        private void UpdateRateOfChange()
        {
            if (UseCustomRateOfChange()) //Is UseControl true on listed or curved RoC modes?
            {
                float changeSpeed = 0f; //The rate of change

                if (listedRoC.UseControl)
                {
                    changeSpeed = listedRoC.GetControlValue(); //We don't increment this here otherwise we'd REALLY end up with an average rate of change of the average of the list
                                                               //When what we actaully want is for the rate of change to update after the current goal intensity is met
                }
                else if (curvedRoC.UseControl)
                {
                    curvedRoC.IncrementControlValue(); //I know, I know, if you read the above comment then this might seem counter intuitive but hear me out
                                                       //When you have a curve rather than a list, not only are you EXPECTING the value to be contantly changing,
                                                       //the change is much smoother. In order to replicate this on a list, you'd need an EXTREEM amount of elements

                    changeSpeed = curvedRoC.GetControlValue();
                }
                else
                {
                    Debug.LogError("Using a custom rate of change with no rate of change enabled. This is a bug, please let the developer know."); //I don't think this is even possible right now, see UseCustomRateOfChange() for details on why

                    return;
                }

                if (changeSpeed < 0f)
                {
                    Debug.LogWarning("Your rate of change became negative, this is not allowed, will use 0 instead. . . Recommended that you fix this. . .");

                    changeSpeed = 0f; //We can't allow a negative rate of change
                }

                if (intensityControlOption.RoCMode == RoCMode.Value) //fix this
                {
                    currentRateOfChange = changeSpeed * Time.deltaTime;
                }
                else// if (intensityControlOption.RoCMode == RoCMode.Time)
                {
                    float percentChanged = changeSpeed > 0
                            ? Time.deltaTime / changeSpeed
                            : 0f;

                    if (curvedIntensities.UseControl && curvedIntensities.CurveMode == ParticleSystemCurveMode.Curve)
                    {
                        float timeChange = curvedIntensities.GetTotalCycleTime().Value * percentChanged;

                        iterationFinished = curvedIntensities.IncrementControlValue(timeChange);

                        currentRateOfChange = Mathf.Abs(currentIntensity - curvedIntensities.GetControlValue());
                    }
                    else
                    {
                        currentRateOfChange = Mathf.Abs(totalChange * percentChanged);//totalChange * percentChanged;
                    }
                }
            }
            else
            {
                if (curvedIntensities.UseControl && curvedIntensities.CurveMode == ParticleSystemCurveMode.Curve)
                {
                    iterationFinished = curvedIntensities.IncrementControlValue(Time.deltaTime);

                    currentRateOfChange = Mathf.Abs(currentIntensity - curvedIntensities.GetControlValue());
                }
                else
                {
                    currentRateOfChange = Mathf.Abs(goalIntensity - currentIntensity);
                }
            }
        }

        private void UpdateIntensityGoal()
        {
            if (currentIntensity == goalIntensity)
            {
                goalIntensity = GetNextIntensityGoal();

                totalChange = Mathf.Abs(currentIntensity - goalIntensity);

                if (listedRoC.UseControl)
                {
                    listedRoC.IncrementControlValue();
                }
            }
            else if (!(curvedIntensities.UseControl && curvedIntensities.CurveMode == ParticleSystemCurveMode.Curve && intensityControlOption.RoCMode == RoCMode.Time))
            {
                iterationFinished = false;
            }
        }

        private float GetNextIntensityGoal()
        {
            float nextGoal = currentIntensity;

            if (curvedIntensities.UseControl)
            {
                if (curvedIntensities.CurveMode == ParticleSystemCurveMode.Curve)
                {
                    if (intensityControlOption.RoCMode == RoCMode.Value)
                    {
                        iterationFinished = curvedIntensities.IncrementControlValue(curvedIntensities.GetTotalCycleTime().Value); //If the change was value based time wasn't being incremented.
                                                                                                                                  //Incrementing it by the whole curve time will ensure the number is high enough--the curve control group won't increment past the goal value
                    }

                    nextGoal = curvedIntensities.GetCurrentMinMaxValue();
                }
                else
                {
                    iterationFinished = curvedIntensities.IncrementControlValue();
                    nextGoal = curvedIntensities.GetControlValue();
                }
            }
            else if (listedIntensities.UseControl)
            {
                iterationFinished = listedIntensities.IncrementControlValue();
                nextGoal = listedIntensities.GetControlValue();
            }

            return nextGoal;
        }

        private void UpdateCurrentIntensity()
        {
            if (currentIntensity < goalIntensity)
            {
                float change = Mathf.Min(goalIntensity - currentIntensity, Mathf.Abs(currentRateOfChange));

                currentIntensity += change;
            }
            if (currentIntensity > goalIntensity)
            {
                float change = Mathf.Min(currentIntensity - goalIntensity, Mathf.Abs(currentRateOfChange));

                currentIntensity -= change;
            }
        }

        public static void SetLightValue(
            IntensityControlTarget valueType,
            float intensity,
            float intensityFloor,
            float intensityCeiling,
            IntensityControlTarget controlTargets,
            ControlTargetModifier[] modifiers,
            ControlOptionGroup controlOptionInfo)
        {
            int length = Mathf.Max(controlOptionInfo.Lights.Length, controlOptionInfo.EmissiveMaterialRenderers.Length);

            for (int i = 0; i < length; i++)
            {
                bool willScale = controlTargets.GetAllFlags().Length > 1;
                int targetIndex = Mathf.FloorToInt(Mathf.Log((int)valueType, 2));

                bool relateInversely = willScale
                    ? modifiers[targetIndex].RelateInversely
                    : false;

                float multiplier = willScale
                    ? modifiers[targetIndex].Multiplier
                    : 1f;

                float offset = willScale
                    ? modifiers[targetIndex].Offset
                    : 0f;
                
                float scaledIntensity = relateInversely
                    ? InverseValue(intensity, intensityFloor, intensityCeiling)
                    : intensity;

                //scaledIntensity = Mathf.Clamp(intensity * multiplier + offset, intensityFloor, intensityCeiling);
                scaledIntensity = Mathf.Clamp(intensity, intensityFloor, intensityCeiling) * multiplier + offset;

                //set value to lights
                switch (valueType)
                {
                    case IntensityControlTarget.LightIntensity:
                        if(i < controlOptionInfo.Lights.Length)
                        {
                            controlOptionInfo.Lights[i].intensity = scaledIntensity;
                        }
                        break;
                    case IntensityControlTarget.LightColorIntensity:
                        if (i < controlOptionInfo.Lights.Length)
                        {
                            controlOptionInfo.Lights[i].color = controlOptionInfo.SaveLightColor
                                ? controlOptionInfo.LightColors[i] * scaledIntensity
                                : controlOptionInfo.Lights[i].color * scaledIntensity;
                        }
                        break;
                    case IntensityControlTarget.MaterialColorIntensity:
                        if(i < controlOptionInfo.EmissiveMaterialRenderers.Length)
                        {
                            for (int k = 0; k < controlOptionInfo.EmissiveMaterialRenderers[i].materials.Length; k++)
                            {
                                Color currentColor = controlOptionInfo.SaveMaterialColor
                                    ? controlOptionInfo.MaterialColors[i][k]
                                    : controlOptionInfo.EmissiveMaterialRenderers[i].materials[k].GetColor("_EmissionColor");

                                controlOptionInfo.EmissiveMaterialRenderers[i].materials[k].SetColor("_EmissionColor", currentColor * scaledIntensity);
                            }
                        }
                        break;
                    case IntensityControlTarget.LightRange:
                        if (i < controlOptionInfo.Lights.Length)
                        {
                            controlOptionInfo.Lights[i].range = scaledIntensity;
                        }
                        break;
                    case IntensityControlTarget.SpotlightAngle:
                        if (i < controlOptionInfo.Lights.Length)
                        {
                            controlOptionInfo.Lights[i].spotAngle = scaledIntensity;
                        }
                        break;
                }
            }

            if(valueType == IntensityControlTarget.LightIntensity)
            {
                controlOptionInfo.IntensityMin = intensityFloor;
                controlOptionInfo.IntensityMax = intensityCeiling;
            }
        }
        
        private void GetMinMax(ControlOptionGroup lightInfo, out float min, out float max)
        {
            float tempMin = 0f;
            float tempMax = 0f;

            if (curvedIntensities.UseControl)
            {
                tempMin = curvedIntensities.MinValue;
                tempMax = curvedIntensities.MaxValue;
            }
            else if (listedIntensities.UseControl)
            {
                tempMin = listedIntensities.MinValue;
                tempMax = listedIntensities.MaxValue;
            }

            min = intensityControlOption.UseLightIntensityFloor
                ? Mathf.Max(intensityControlOption.Floor, tempMin) //chooses the more restrictive one
                : tempMin;

            max = intensityControlOption.UseLightIntensityCeiling
                ? Mathf.Min(intensityControlOption.Ceiling, tempMax) //chooses the more restrictive one
                : tempMax;
        }
        
        public static float InverseValue(float value, float minValue, float maxValue)
        {
            float inversePercent = 1f - (value - minValue) / (maxValue - minValue);

            float inverseValue = inversePercent * (maxValue - minValue) + minValue;

            return inverseValue;
        }
    }
}