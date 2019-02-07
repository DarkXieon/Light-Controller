using LightControls.Controllers;
using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.ControlOptions
{
    public class InstancedColorControlOption : InstancedControlOption
    {
        private ColorControlOption colorControlOption;

        /// <summary>
        /// These variables are used in determining the color that this control option will use
        /// </summary>
        #region Color Generation Variables

        private GradientControlGroup ColorControl; //This gradient control handles tracking the color of the gradient

        #endregion

        #region Rate of Change Generation Variables

        private ListControlGroup ListedRoC;
        private CurveControlGroup CurvedRoC;

        #endregion

        #region Local Variables

        private Color currentColor;
        private Color goalColor;
        private Color totalColorChange;
        private Color currentRateOfChange;
        private bool iterationFinished;

        #endregion

        public InstancedColorControlOption(ColorControlOption option)
        {
            colorControlOption = option;

            ColorControl = new GradientControlGroup(option.ColorControl);
            ListedRoC = new ListControlGroup(option.ListedRoC);
            CurvedRoC = new CurveControlGroup(option.CurvedRoC);

            currentColor = ColorControl.GetCurrentColor();
            goalColor = GetNextColorGoal();
            totalColorChange = goalColor - currentColor;
            currentRateOfChange = new Color(0f, 0f, 0f, 0f);
            iterationFinished = false;
        }

        public override bool UpdateControl()
        {
            bool previousValue = iterationFinished;

            UpdateColorGoal();
            UpdateRateOfChange();
            UpdateCurrentColorPosition();

            return UseCustomRateOfChange() && ColorControl.MinMaxGradient.mode == ParticleSystemGradientMode.Gradient && colorControlOption.RoCMode == RoCMode.Time //under these conditions, the iterations finished variable will be set one update early
                ? previousValue
                : iterationFinished;
        }

        public override bool ApplyOn(ApplicationStages stage)
        {
            if (stage == ApplicationStages.LightColorApplication)
            {
                return colorControlOption.ColorTarget.HasFlag(ColorControlTarget.Light);
            }
            else if (stage == ApplicationStages.MaterialColorApplication)
            {
                return colorControlOption.ColorTarget.HasFlag(ColorControlTarget.Material);
            }

            return false;
        }

        public override void ApplyControl(ControlOptionGroup controlOptionInfo)
        {
            if (controlOptionInfo.CurrentStage == ApplicationStages.LightColorApplication && colorControlOption.ColorTarget.HasFlag(ColorControlTarget.Light))
            {
                ApplyColorToLights(controlOptionInfo.Lights);
            }

            if (controlOptionInfo.CurrentStage == ApplicationStages.MaterialColorApplication && colorControlOption.ColorTarget.HasFlag(ColorControlTarget.Material))
            {
                ApplyColorToMaterials(controlOptionInfo.EmissiveMaterialRenderers);
            }
        }
        
        private void UpdateRateOfChange()
        {
            if (UseCustomRateOfChange())
            {
                float changeSpeed = 0f;

                if (ListedRoC.UseControl)
                {
                    changeSpeed = ListedRoC.GetControlValue();
                }
                else if (CurvedRoC.UseControl)
                {
                    CurvedRoC.IncrementControlValue();

                    changeSpeed = CurvedRoC.GetControlValue();
                }
                else
                {
                    Debug.LogError("Using a custom rate of change with no rate of change enabled. This is a bug, please let the developer know.");

                    return;
                }

                changeSpeed = Mathf.Max(0f, changeSpeed);

                if (colorControlOption.RoCMode == RoCMode.Value)
                {
                    currentRateOfChange = new Color(changeSpeed * Time.deltaTime, changeSpeed * Time.deltaTime, changeSpeed * Time.deltaTime);
                }
                else if (colorControlOption.RoCMode == RoCMode.Time)
                {
                    float percentChanged = changeSpeed > 0
                        ? Time.deltaTime / changeSpeed
                        : 0f;

                    if (ColorControl.MinMaxGradient.mode == ParticleSystemGradientMode.Gradient)
                    {
                        float timeChange = percentChanged;

                        iterationFinished = ColorControl.IncrementColor(timeChange);

                        currentRateOfChange = PositiveColor(currentColor - ColorControl.GetCurrentColor());
                    }
                    else
                    {
                        currentRateOfChange = totalColorChange * percentChanged;
                    }
                }
            }
            else
            {
                currentRateOfChange = PositiveColor(goalColor - currentColor);
            }
        }

        private void UpdateColorGoal()
        {
            if (currentColor == goalColor)
            {
                goalColor = GetNextColorGoal();

                totalColorChange = PositiveColor(currentColor - goalColor);

                if (ListedRoC.UseControl)
                {
                    ListedRoC.IncrementControlValue();
                }
            }
            else if (!(UseCustomRateOfChange() && ColorControl.MinMaxGradient.mode == ParticleSystemGradientMode.Gradient && colorControlOption.RoCMode == RoCMode.Time))
            {
                iterationFinished = false;
            }
        }

        public void UpdateCurrentColorPosition()
        {
            currentColor = GetChanged(currentColor, goalColor, currentRateOfChange);
        }

        public bool UseCustomRateOfChange()
        {
            return CurvedRoC.UseControl || ListedRoC.UseControl;
        }

        private Color GetNextColorGoal()
        {
            Color nextGoal = goalColor;

            if (UseCustomRateOfChange() && ColorControl.MinMaxGradient.mode == ParticleSystemGradientMode.Gradient)
            {
                if (colorControlOption.RoCMode == RoCMode.Value)
                {
                    iterationFinished = ColorControl.IncrementColor(1f); //If the change was value based time wasn't being incremented.
                                                                         //Incrementing it by the whole gradient time will ensure the number is high enough--the gradient control group won't increment past the goal value
                }

                nextGoal = ColorControl.GetCurrentMinMaxColor();
            }
            else
            {
                iterationFinished = ColorControl.IncrementColor();
                nextGoal = ColorControl.GetCurrentColor();
            }

            return nextGoal;
        }

        private Color PositiveColor(Color color)
        {
            return new Color(Mathf.Abs(color.r), Mathf.Abs(color.g), Mathf.Abs(color.b), Mathf.Abs(color.a));
        }

        private Color GetChanged(Color current, Color to, Color change)
        {
            current.r = to.r > current.r
                ? Mathf.Clamp(current.r + change.r, current.r, to.r)
                : Mathf.Clamp(current.r - change.r, to.r, current.r);

            current.g = to.g > current.g
                ? Mathf.Clamp(current.g + change.g, current.g, to.g)
                : Mathf.Clamp(current.g - change.g, to.g, current.g);

            current.b = to.b > current.b
                ? Mathf.Clamp(current.b + change.b, current.b, to.b)
                : Mathf.Clamp(current.b - change.b, to.b, current.b);

            current.a = to.a > current.a
                ? Mathf.Clamp(current.a + change.a, current.a, to.a)
                : Mathf.Clamp(current.a - change.a, to.a, current.a);

            return current;
        }

        private void ApplyColorToLights(Light[] lights)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].color = currentColor;
            }
        }

        private void ApplyColorToMaterials(Renderer[] renderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int k = 0; k < renderers[i].materials.Length; k++)
                {
                    if (Application.isPlaying)
                    {
                        renderers[i].materials[k].EnableKeyword("_EMISSION");

                        renderers[i].materials[k].SetColor("_EmissionColor", currentColor);
                    }
                    else
                    {
                        renderers[i].sharedMaterials[k].EnableKeyword("_EMISSION");

                        renderers[i].sharedMaterials[k].SetColor("_EmissionColor", currentColor);
                    }
                }

                renderers[i].UpdateGIMaterials();
            }
        }
    }
}