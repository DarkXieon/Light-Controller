using System;
using System.Linq;

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

        private IntensityControlTarget previousTarget;
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

            previousTarget = intensityControlOption.ControlTarget;

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

            //bool finished = curvedIntensities.UseControl && curvedIntensities.CurveMode == ParticleSystemCurveMode.Curve && intensityControlOption.RoCMode == RoCMode.Time //under these conditions, the iterations finished variable will be set one update early
            //    ? previousValue
            //    : iterationFinished;
            
            return curvedIntensities.UseControl && curvedIntensities.CurveMode == ParticleSystemCurveMode.Curve && intensityControlOption.RoCMode == RoCMode.Time //under these conditions, the iterations finished variable will be set one update early
                ? previousValue
                : iterationFinished;
        }

        public override bool ApplyOn(ApplicationStages stage)
        {
            if (stage == ApplicationStages.IntensityApplication)
            {
                return intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.LightIntensity);
            }
            if (stage == ApplicationStages.ColorIntensityApplication)
            {
                return intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.LightColorIntensity) || intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.MaterialColorIntensity);
            }
            if (stage == ApplicationStages.OtherApplications)
            {
                return intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.LightRange) || intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.SpotlightAngle);
            }

            return false;
        }

        public override void ApplyControl(ControlOptionInfo controlOptionInfo)
        {
            PrepareTargets(controlOptionInfo);

            if (controlOptionInfo.CurrentStage == ApplicationStages.IntensityApplication && intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.LightIntensity))
            {
                SetLightIntensity(controlOptionInfo, currentIntensity);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.ColorIntensityApplication && intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.LightColorIntensity))
            {
                SetLightColorIntensity(controlOptionInfo, currentIntensity);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.ColorIntensityApplication && intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.MaterialColorIntensity))
            {
                SetMaterialColorIntensity(controlOptionInfo, currentIntensity);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.OtherApplications && intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.LightRange))
            {
                SetLightRange(controlOptionInfo, currentIntensity);
            }
            if (controlOptionInfo.CurrentStage == ApplicationStages.OtherApplications && intensityControlOption.ControlTarget.HasFlag(IntensityControlTarget.SpotlightAngle))
            {
                SetLightSpotAngle(controlOptionInfo, currentIntensity);
            }
        }

        public bool UseCustomRateOfChange()
        {
            return intensityControlOption.UseCustomRateOfChange;
        }
        
        private void PrepareTargets(ControlOptionInfo controlOptionInfo)
        {
            IntensityControlTarget xOrTarget = intensityControlOption.ControlTarget ^ previousTarget;
            previousTarget = intensityControlOption.ControlTarget;

            IntensityControlTarget[] targets = xOrTarget.GetAllFlags();

            if (controlOptionInfo.SaveLightColor && targets.Contains(IntensityControlTarget.LightColorIntensity))
            {
                for (int i = 0; i < controlOptionInfo.Lights.Length; i++)
                {
                    controlOptionInfo.Lights[i].color = controlOptionInfo.LightColors[i];
                }
            }
            if (controlOptionInfo.SaveMaterialColor && targets.Contains(IntensityControlTarget.MaterialColorIntensity))
            {
                for (int i = 0; i < controlOptionInfo.EmissiveMaterialRenderers.Length; i++)
                {
                    for (int k = 0; k < controlOptionInfo.EmissiveMaterialRenderers[i].materials.Length; k++)
                    {
                        controlOptionInfo.EmissiveMaterialRenderers[i].material.EnableKeyword("_EMISSION");
                        controlOptionInfo.EmissiveMaterialRenderers[i].UpdateGIMaterials();

                        Color currentColor = controlOptionInfo.MaterialColors[i][k];
                        controlOptionInfo.EmissiveMaterialRenderers[i].materials[k].SetColor("_EmissionColor", currentColor);
                    }
                }
            }
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
            //TODO: FIX THIS... SEE COLOR CONTROL OPTIONS FOR DETAILS
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

        private void SetLightIntensity(ControlOptionInfo controlOptionInfo, float value)
        {
            bool reachedIntensityGoal = currentIntensity == goalIntensity;

            if (!intensityControlOption.NoSmoothTransitions || (intensityControlOption.NoSmoothTransitions && reachedIntensityGoal))
            {
                for (int i = 0; i < controlOptionInfo.Lights.Length; i++)
                {
                    if (controlOptionInfo.Lights[i] == null)
                    {
                        continue;
                    }

                    bool willScale = intensityControlOption.ControlTarget.GetAllFlags().Length > 1;

                    value = willScale && intensityControlOption.IntensityModifers[0].RelateInversely
                        ? InverseValue(value)
                        : value;

                    float scaledValue = willScale
                        ? value * intensityControlOption.IntensityModifers[0].Multiplier + intensityControlOption.IntensityModifers[0].Offset
                        : value;

                    scaledValue = intensityControlOption.UseLightIntensityFloor && scaledValue < intensityControlOption.Floor
                        ? intensityControlOption.Floor
                        : scaledValue;

                    scaledValue = intensityControlOption.UseLightIntensityCeiling && scaledValue > intensityControlOption.Ceiling
                        ? intensityControlOption.Ceiling
                        : scaledValue;

                    controlOptionInfo.Lights[i].intensity = scaledValue;
                }

                SetMinMaxs(controlOptionInfo);
            }
        }

        private void SetLightColorIntensity(ControlOptionInfo controlOptionInfo, float value)
        {
            bool reachedIntensityGoal = currentIntensity == goalIntensity;

            if (!intensityControlOption.NoSmoothTransitions || (intensityControlOption.NoSmoothTransitions && reachedIntensityGoal))
            {
                for (int i = 0; i < controlOptionInfo.Lights.Length; i++)
                {
                    if (controlOptionInfo.Lights[i] == null)
                    {
                        continue;
                    }

                    if (controlOptionInfo.SaveLightColor)
                    {
                        controlOptionInfo.Lights[i].color = controlOptionInfo.LightColors[i];
                    }

                    bool willScale = intensityControlOption.ControlTarget.GetAllFlags().Length > 1;

                    value = willScale && intensityControlOption.IntensityModifers[1].RelateInversely
                        ? InverseValue(value)
                        : value;

                    float scaledValue = willScale
                        ? value * intensityControlOption.IntensityModifers[1].Multiplier + intensityControlOption.IntensityModifers[1].Offset
                        : value;

                    scaledValue = intensityControlOption.UseLightIntensityFloor && scaledValue < intensityControlOption.Floor
                        ? intensityControlOption.Floor
                        : scaledValue;

                    scaledValue = intensityControlOption.UseLightIntensityCeiling && scaledValue > intensityControlOption.Ceiling
                        ? intensityControlOption.Ceiling
                        : scaledValue;

                    controlOptionInfo.Lights[i].color *= scaledValue;
                }
            }
        }

        private void SetMaterialColorIntensity(ControlOptionInfo controlOptionInfo, float value)
        {
            bool reachedIntensityGoal = currentIntensity == goalIntensity;

            if (!intensityControlOption.NoSmoothTransitions || (intensityControlOption.NoSmoothTransitions && reachedIntensityGoal))
            {
                for (int i = 0; i < controlOptionInfo.EmissiveMaterialRenderers.Length; i++)
                {
                    if (controlOptionInfo.EmissiveMaterialRenderers[i] == null)
                    {
                        continue;
                    }

                    if (controlOptionInfo.SaveMaterialColor)
                    {
                        controlOptionInfo.EmissiveMaterialRenderers[i].material.EnableKeyword("_EMISSION");
                        controlOptionInfo.EmissiveMaterialRenderers[i].UpdateGIMaterials();

                        for (int k = 0; k < controlOptionInfo.EmissiveMaterialRenderers[i].materials.Length && k < controlOptionInfo.MaterialColors[i].Length; k++)
                        {
                            if (controlOptionInfo.EmissiveMaterialRenderers[i].materials[k] != null)
                                controlOptionInfo.EmissiveMaterialRenderers[i].materials[k].SetColor("_EmissionColor", controlOptionInfo.MaterialColors[i][k]);
                        }
                    }

                    bool willScale = intensityControlOption.ControlTarget.GetAllFlags().Length > 1;

                    value = willScale && intensityControlOption.IntensityModifers[2].RelateInversely
                        ? InverseValue(value)
                        : value;

                    float scaledValue = willScale
                        ? value * intensityControlOption.IntensityModifers[2].Multiplier + intensityControlOption.IntensityModifers[2].Offset
                        : value;

                    scaledValue = intensityControlOption.UseLightIntensityFloor && scaledValue < intensityControlOption.Floor
                        ? intensityControlOption.Floor
                        : scaledValue;

                    scaledValue = intensityControlOption.UseLightIntensityCeiling && scaledValue > intensityControlOption.Ceiling
                        ? intensityControlOption.Ceiling
                        : scaledValue;

                    for (int k = 0; k < controlOptionInfo.EmissiveMaterialRenderers[i].materials.Length; k++)
                    {
                        Color currentColor = controlOptionInfo.EmissiveMaterialRenderers[i].materials[k].GetColor("_EmissionColor");

                        controlOptionInfo.EmissiveMaterialRenderers[i].materials[k].SetColor("_EmissionColor", currentColor * scaledValue);
                    }
                }
            }
        }

        private void SetLightRange(ControlOptionInfo controlOptionInfo, float value)
        {
            bool reachedIntensityGoal = currentIntensity == goalIntensity;

            if (!intensityControlOption.NoSmoothTransitions || (intensityControlOption.NoSmoothTransitions && reachedIntensityGoal))
            {
                for (int i = 0; i < controlOptionInfo.Lights.Length; i++)
                {
                    if (controlOptionInfo.Lights[i] == null)
                    {
                        continue;
                    }

                    bool willScale = intensityControlOption.ControlTarget.GetAllFlags().Length > 1;

                    value = willScale && intensityControlOption.IntensityModifers[3].RelateInversely
                        ? InverseValue(value)
                        : value;

                    float scaledValue = willScale
                        ? value * intensityControlOption.IntensityModifers[3].Multiplier + intensityControlOption.IntensityModifers[3].Offset
                        : value;

                    scaledValue = intensityControlOption.UseLightIntensityFloor && scaledValue < intensityControlOption.Floor
                        ? intensityControlOption.Floor
                        : scaledValue;

                    scaledValue = intensityControlOption.UseLightIntensityCeiling && scaledValue > intensityControlOption.Ceiling
                        ? intensityControlOption.Ceiling
                        : scaledValue;

                    controlOptionInfo.Lights[i].range = scaledValue;
                }
            }
        }

        private void SetLightSpotAngle(ControlOptionInfo controlOptionInfo, float value)
        {
            bool reachedIntensityGoal = currentIntensity == goalIntensity;

            if (!intensityControlOption.NoSmoothTransitions || (intensityControlOption.NoSmoothTransitions && reachedIntensityGoal))
            {
                for (int i = 0; i < controlOptionInfo.Lights.Length; i++)
                {
                    if (controlOptionInfo.Lights[i] == null)
                    {
                        continue;
                    }

                    bool willScale = intensityControlOption.ControlTarget.GetAllFlags().Length > 1;

                    value = willScale && intensityControlOption.IntensityModifers[4].RelateInversely
                        ? InverseValue(value)
                        : value;

                    float scaledValue = willScale
                        ? value * intensityControlOption.IntensityModifers[4].Multiplier + intensityControlOption.IntensityModifers[4].Offset
                        : value;

                    scaledValue = intensityControlOption.UseLightIntensityFloor && scaledValue < intensityControlOption.Floor
                        ? intensityControlOption.Floor
                        : scaledValue;

                    scaledValue = intensityControlOption.UseLightIntensityCeiling && scaledValue > intensityControlOption.Ceiling
                        ? intensityControlOption.Ceiling
                        : scaledValue;

                    controlOptionInfo.Lights[i].spotAngle = scaledValue;
                }
            }
        }

        private void SetMinMaxs(ControlOptionInfo lightInfo)
        {
            if (curvedIntensities.UseControl)
            {
                lightInfo.IntensityMin = curvedIntensities.MinValue;
                lightInfo.IntensityMax = curvedIntensities.MaxValue;
            }
            else if (listedIntensities.UseControl)
            {
                lightInfo.IntensityMin = listedIntensities.MinValue;
                lightInfo.IntensityMax = listedIntensities.MaxValue;
            }
            else
            {
                lightInfo.IntensityMin = 0f;
                lightInfo.IntensityMax = 0f;
            }
        }

        private float InverseValue(float value)
        {
            float inversePercent = 1f - (value - intensityControlOption.MinValue) / (intensityControlOption.MaxValue - intensityControlOption.MinValue);

            float inverseValue = inversePercent * (intensityControlOption.MaxValue - intensityControlOption.MinValue) + intensityControlOption.MinValue;

            return inverseValue;
        }
    }
}