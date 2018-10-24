#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;

using LightControls.Controllers;
using LightControls.ControlOptions;
using LightControls.ControlOptions.ControlGroups;
using UnityEngine;

namespace LightControls.Utilities
{
    public static class WarningSystem
    {
        public static void DisplayStagedLightControllerWarnings(StagedLightController stagedLightController)
        {
            var stagedWrong = GetRecursive(stagedLightController)
                .Where(option => HasClampedIteration(option));

            bool incorrectStagingDectected = stagedWrong.Any();

            if (incorrectStagingDectected)
            {
                string[] names = Object.FindObjectsOfType<StagedLightController>()
                    .Where(controller => controller.ControllerStager.Stages
                        .Any(stage => stage.LightControllers
                            .OfType<GroupedLightController>()
                            .Any(control => control.ControlInfo
                                .Any(info => info.LightControlOptions
                                    .Any(option => stagedWrong.Contains(option))))))
                    .Select(controller => controller.name)
                    .ToArray();

                string nameString = string.Empty;

                names.ToList().ForEach(name => nameString += name + ", ");

                nameString.TrimEnd(' ');
                nameString.TrimEnd(',');

                string possessive = names.Length > 1
                    ? "have"
                    : "has";

                Debug.LogWarning(string.Format("{0} {1} clamped iteration which will halt the stages from advancing. Advise switching the curve end mode to ping-pong or loop", nameString, possessive));
            }
        }

        private static IEnumerable<LightControlOption> GetRecursive(StagedLightController controller)
        {
            if (controller.ControllerStager.Stages.Length > 0)
            {
                return controller.ControllerStager.Stages
                    .SelectMany(stage => stage.LightControllers)
                    .SelectMany(control =>
                    {
                        if (control is StagedLightController)
                            return GetRecursive((StagedLightController)control);
                        else
                            return GetRecursive((GroupedLightController)control);
                    });
            }
            else
            {
                return new LightControlOption[0];
            }
        }

        private static IEnumerable<LightControlOption> GetRecursive(GroupedLightController controller)
        {
            if (controller.ControlInfo.Length > 0)
            {
                return controller.ControlInfo
                    .SelectMany(info =>
                    {
                        if (info.LightControlOptions.Length > 0)
                        {
                            var final = new List<LightControlOption>();

                            var nonStaged = info.LightControlOptions
                                .Where(option => !(option is StagedControlOption));

                            final.AddRange(nonStaged);

                            var staged = info.LightControlOptions
                                .OfType<StagedControlOption>();

                            foreach (StagedControlOption stagedControl in staged)
                            {
                                final.AddRange(GetRecursive(stagedControl));
                            }

                            return final;
                        }
                        else
                        {
                            return new List<LightControlOption>();
                        }
                    });
            }
            else
            {
                return new LightControlOption[0];
            }
        }

        private static IEnumerable<LightControlOption> GetRecursive(StagedControlOption stagedOption)
        {
            if (stagedOption.Stager.Stages.Length > 0)
            {
                return stagedOption.Stager.Stages
                    .SelectMany(stage =>
                    {
                        if (stage.ControlOptions.Length > 0)
                        {
                            var final = new List<LightControlOption>();

                            var nonStaged = stage.ControlOptions
                                .Where(option => !(option is StagedControlOption));

                            final.AddRange(nonStaged);

                            var staged = stage.ControlOptions
                                .OfType<StagedControlOption>();

                            foreach (StagedControlOption stagedControl in staged)
                            {
                                final.AddRange(GetRecursive(stagedControl));
                            }

                            return final;
                        }
                        else
                        {
                            return new List<LightControlOption>();
                        }
                    });
            }
            else
            {
                return new LightControlOption[0];
            }
        }


        private static bool HasClampedIteration(LightControlOption option)
        {
            if (option is IntensityControlOption)
            {
                IntensityControlOption casted = (IntensityControlOption)option;

                if (casted.CurvedIntensities.UseControl)
                {
                    if (casted.CurvedIntensities.MinMaxCurve.mode == ParticleSystemCurveMode.Curve)
                    {
                        return casted.CurvedIntensities.MaxCurveTracker.WrapMode == MinMaxWrapMode.Clamp;
                    }
                    else if (casted.CurvedIntensities.MinMaxCurve.mode == ParticleSystemCurveMode.TwoCurves)
                    {
                        return casted.CurvedIntensities.MaxCurveTracker.WrapMode == MinMaxWrapMode.Clamp || casted.CurvedIntensities.MinCurveTracker.WrapMode == MinMaxWrapMode.Clamp;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (option is ColorControlOption)
            {
                ColorControlOption casted = (ColorControlOption)option;

                if (casted.ColorControl.MinMaxGradient.mode == ParticleSystemGradientMode.Gradient)
                {
                    return casted.ColorControl.MinMaxTracker.WrapMode == MinMaxWrapMode.Clamp;
                }
                else if (casted.ColorControl.MinMaxGradient.mode == ParticleSystemGradientMode.TwoGradients)
                {
                    return casted.ColorControl.MinMaxTracker.WrapMode == MinMaxWrapMode.Clamp || casted.ColorControl.MinMaxTracker.WrapMode == MinMaxWrapMode.Clamp;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}

#endif