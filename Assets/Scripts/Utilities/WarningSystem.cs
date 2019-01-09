#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightControls.Controllers;
using LightControls.ControlOptions;
using LightControls.ControlOptions.ControlGroups;
using LightControls.ControlOptions.Stages;
using UnityEngine;

namespace LightControls.Utilities
{
    public class StagedLightControllerEntry
    {
        public LightController[][] StagesLightControllers;
        public bool Changed = false;
    }
    
    public static partial class WarningSystem
    {
        public static void DisplayStagedLightControllerWarnings(StagedLightController stagedLightController)
        {
            IEnumerable<WarningInfo> testWarnings;

            HasWarnings(stagedLightController, 0, out testWarnings);
            
            if(testWarnings.Any())
            {
                StringBuilder warningMessageBuilder = new StringBuilder();

                warningMessageBuilder.AppendLine(string.Format("There are some staging issues with {0} the one or more stages will be effectively skipped if they remain the way they are", stagedLightController.name));

                var lightControllerStages = testWarnings.Where(warning => warning.IsController && warning.IsStaged);
                var groupedLightControllers = testWarnings.Where(warning => warning.IsController && !warning.IsStaged);

                var controlOptionStages = testWarnings.Where(warning => !warning.IsController && warning.IsStaged);
                var normalControlOptions = testWarnings.Where(warning => !warning.IsController && !warning.IsStaged);

                foreach(WarningInfo info in lightControllerStages)
                {
                    //warningMessageBuilder.AppendLine(string.Format("Issues in {0} stage {1}:", info.Controller.name, info.StageIndex + 1));

                    var controllersInStage = groupedLightControllers.Where(controller => ((LightControllerStage)info.Stage).LightControllers.Contains(controller.Controller));

                    foreach(WarningInfo controllerInfo in controllersInStage)
                    {
                        var optionsInStage = controlOptionStages
                            .Concat(normalControlOptions)
                            .Where(warning => ((GroupedLightController)controllerInfo.Controller).ControlInfo
                                .SelectMany(controlInfo => controlInfo.LightControlOptions)
                                .Any(option => option == warning.Option));

                        if (optionsInStage.Any())
                        {
                            var normalOptionsInStage = optionsInStage.Where(warning => !warning.IsStaged);
                            var stagedOptionsInStage = optionsInStage.Except(normalControlOptions);

                            foreach (WarningInfo normalInfo in normalOptionsInStage)
                            {
                                warningMessageBuilder.AppendLine(string.Format("Control Option '{0}' in  controller '{1}' stage {2} entry '{3}'", normalInfo.Option.name, info.Controller.name, info.StageIndex + 1, controllerInfo.Controller.name));
                            }

                            foreach (WarningInfo stagedInfo in stagedOptionsInStage)
                            {
                                var controlsInOption = normalControlOptions.Where(warning => ((ControlOptionStage)stagedInfo.Stage).ControlOptions.Contains(warning.Option));
                                
                                foreach (WarningInfo optionInfo in controlsInOption)
                                {
                                    warningMessageBuilder.AppendLine(string.Format("Control Option '{0}' in controller '{1}', stage {2} entry '{3}'", optionInfo.Option.name, stagedInfo.Option.name, stagedInfo.StageIndex + 1, controllerInfo.Controller.name));
                                }
                            }
                        }
                    }
                }

                Debug.LogWarning(warningMessageBuilder.ToString());
            }

            var toCheck = testWarnings.Where(warning => (warning.IsController && !warning.IsStaged) || !warning.IsController);
            
            foreach (WarningInfo info in toCheck)
            {
                var controllersContaining = info.IsController
                    ? testWarnings.Where(warning => warning.IsController && warning.IsStaged && ((LightControllerStage)warning.Stage).LightControllers.Contains(info.Controller))
                    : testWarnings.Where(warning =>
                        (warning.IsController && !warning.IsStaged && ((GroupedLightController)warning.Controller).ControlInfo.Any(controlInfo => controlInfo.LightControlOptions.Contains(info.Option)))
                     || (!warning.IsController && warning.IsStaged && ((ControlOptionStage)warning.Stage).ControlOptions.Contains(info.Option)));

                controllersContaining.ToList().ForEach(containing => Debug.Log(string.Format("StageName: {0} Index: {1} ControlName: {2}{3}", containing.IsController ? containing.Controller.name : containing.Option.name, containing.StageIndex, info.IsController ? info.Controller.name : info.Option.name, "\n")));
            }
        }

        private class WarningInfo
        {
            public bool IsController;
            public LightController Controller;

            public LightControlOption Option;

            public int Depth;

            public bool IsStaged;
            public Stage Stage;
            public int StageIndex;
        }
        
        private static bool HasWarnings(LightController controller, int depth, out IEnumerable<WarningInfo> warnings)
        {
            if (controller is StagedLightController)
            {
                StagedLightController stagedController = (StagedLightController)controller;

                List<WarningInfo> infos = new List<WarningInfo>();

                for (int i = 0; i < stagedController.ControllerStager.Stages.Length; i++)
                {
                    LightControllerStage currentStage = stagedController.ControllerStager.Stages[i];
                    List<WarningInfo> totalWarnings = new List<WarningInfo>();

                    bool hasWarnings = currentStage.LightControllers
                        .All(currentController =>
                        {
                            IEnumerable<WarningInfo> stageInfo;
                            bool warned = HasWarnings(currentController, depth + 1, out stageInfo);
                            totalWarnings.AddRange(stageInfo);
                            return warned;
                        });

                    if (hasWarnings)
                    {
                        WarningInfo info = new WarningInfo()
                        {
                            IsController = true,
                            Controller = stagedController,
                            Depth = depth,
                            IsStaged = true,
                            Stage = currentStage,
                            StageIndex = i
                        };

                        infos.Add(info);
                        infos.AddRange(totalWarnings);
                    }
                }

                warnings = infos;

                return infos.Any();
            }
            else
            {
                GroupedLightController groupedController = (GroupedLightController)controller;

                List<WarningInfo> infos = new List<WarningInfo>();

                bool hasWarnings = groupedController.ControlInfo
                    .SelectMany(info => info.LightControlOptions)
                    .All(option =>
                    {
                        IEnumerable<WarningInfo> optionWarnings;
                        bool warned = HasWarnings(option, depth + 1, out optionWarnings);
                        infos.AddRange(optionWarnings);
                        return warned;
                    });

                if(hasWarnings)
                {
                    WarningInfo info = new WarningInfo()
                    {
                        IsController = true,
                        Controller = groupedController,
                        Depth = depth,
                        IsStaged = false
                    };

                    infos.Add(info);

                    warnings = infos;
                }
                else
                {
                    warnings = new WarningInfo[0];
                }

                return hasWarnings;
            }
        }

        private static bool HasWarnings(LightControlOption option, int depth, out IEnumerable<WarningInfo> warnings)
        {
            if (option is StagedControlOption)
            {
                StagedControlOption stagedOption = (StagedControlOption)option;

                List<WarningInfo> infos = new List<WarningInfo>();

                for (int i = 0; i < stagedOption.Stager.Stages.Length; i++)
                {
                    ControlOptionStage currentStage = stagedOption.Stager.Stages[i];
                    List<WarningInfo> totalWarnings = new List<WarningInfo>();

                    bool hasWarnings = currentStage.ControlOptions
                        .All(currentController =>
                        {
                            IEnumerable<WarningInfo> stageInfo;
                            bool warned = HasWarnings(currentController, depth + 1, out stageInfo);
                            totalWarnings.AddRange(stageInfo);
                            return warned;
                        });

                    if (hasWarnings)
                    {
                        WarningInfo info = new WarningInfo()
                        {
                            IsController = false,
                            Option = stagedOption,
                            Depth = depth,
                            IsStaged = true,
                            Stage = currentStage,
                            StageIndex = i
                        };

                        infos.Add(info);
                        infos.AddRange(totalWarnings);
                    }
                }
                
                warnings = infos;

                return infos.Any();
            }
            else
            {
                WarningInfo info = null;

                if (HasClampedIteration(option))
                {
                    info = new WarningInfo()
                    {
                        IsController = false,
                        Option = option,
                        Depth = depth,
                        IsStaged = false
                    };
                }

                warnings = new WarningInfo[] { info };

                return info != null;
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
    }
}

#endif