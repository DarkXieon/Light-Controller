using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using LightControls.ControlOptions;
using LightControls.ControlOptions.ControlGroups;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEngine;

namespace LightControls.Controllers
{
    public class StageEventInfo
    {
        public bool IsDurationDone { get; private set; }

        public StageEventInfo(bool isDurationDone)
        {
            IsDurationDone = isDurationDone;
        }

        //public LightController current;
        //public LightController next;
    }

    [System.Serializable]
    public class EventDetector
    {
        public delegate bool EventDetectDelegate(StageEventInfo detectInfo);

        public MonoBehaviour Detector => detector;
        public string MethodName => methodName;

        [SerializeField] private MonoBehaviour detector;
        [SerializeField] private string methodName = string.Empty;

        private MonoBehaviour instancedValue;
        private EventDetectDelegate found;

        public bool EventDetected(StageEventInfo detectInfo)
        {
            Debug.Assert(Application.isPlaying);

            if (detector != null && instancedValue == null)
            {
                instancedValue = detector.gameObject.scene.name == null
                    ? UnityEngine.Object.Instantiate(detector)
                    : detector;
            }

            if (!string.IsNullOrWhiteSpace(methodName))
            {
                if (found == null || found.GetInvocationList().Length < 1 || found.GetInvocationList()[0].Method.Name != methodName)
                {
                    found = FindMethod();
                }

                return found.Invoke(detectInfo);
            }

            return false;
        }

        private EventDetectDelegate FindMethod()
        {
            MethodInfo methodInfo = FindMatchingMethod(methodName, instancedValue);

            return (EventDetectDelegate)Delegate.CreateDelegate(typeof(EventDetectDelegate), instancedValue, methodInfo);
        }
        
        public static IEnumerable<MethodInfo> FindMatchingMethods(MonoBehaviour script = null)
        {
            MethodInfo[] foundMethods = script == null
                ? typeof(StageEventHandlers).GetMethods(BindingFlags.Public | BindingFlags.Static)
                : script.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            
            return foundMethods
                .Where(method => method.ReturnType == typeof(System.Boolean))
                .Where(method => method.GetParameters().Count() == 1 && method.GetParameters()[0].ParameterType == typeof(StageEventInfo));
        }

        public static MethodInfo FindMatchingMethod(string name, MonoBehaviour script = null)
        {
            return FindMatchingMethods(script)
                .Where(method => method.Name == name)
                .SingleOrDefault();
        }
    }

    public class StagedLightController : LightController
    {
        public override ILightControllerInfo[] LightControllerInfo => new InstancedLightControllerStager[1] { instancedStager };

        public LightControllerStager ControllerStager = new LightControllerStager();

        private InstancedLightControllerStager instancedStager;

        private void Awake()
        {
            ControllerStager = ControllerStager ?? new LightControllerStager();

            instancedStager = this.enabled 
                ? new InstancedLightControllerStager(ControllerStager)
                : new InstancedLightControllerStager(new LightControllerStager());

            //todo: move this out of this class
            DisplayWarnings();
        }

        public void UpdateInstancedController()
        {
            instancedStager.UpdateInstanced();
        }

        private void DisplayWarnings()
        {
            var stagedWrong = GetRecursive(this)
                .Where(option => HasClampedIteration(option));

            bool incorrectStagingDectected = stagedWrong.Any();
            
            if(incorrectStagingDectected)
            {
                string[] names = FindObjectsOfType<StagedLightController>()
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

                UnityEngine.Debug.LogWarning(string.Format("{0} {1} clamped iteration which will halt the stages from advancing. Advise switching the curve end mode to ping-pong or loop", nameString, possessive));
            }
        }

        private IEnumerable<LightControlOption> GetRecursive(StagedLightController controller)
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

        private IEnumerable<LightControlOption> GetRecursive(GroupedLightController controller)
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

        private IEnumerable<LightControlOption> GetRecursive(StagedControlOption stagedOption)
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

        private IEnumerable<LightControlOption> GetLastRecursive(StagedLightController controller)
        {
            if(controller.ControllerStager.Stages.Length > 0)
            {
                LightControllerStage last = controller.ControllerStager.Stages.Last();

                return last.LightControllers
                    .SelectMany(control =>
                    {
                        if (control is StagedLightController)
                            return GetLastRecursive((StagedLightController)control);
                        else
                            return GetRecursive((GroupedLightController)control);
                    });
            }
            else
            {
                return new LightControlOption[0];
            }
        }

        //private IEnumerable<LightControlOption> GetLastRecursive(GroupedLightController controller)
        //{
        //    if (controller.ControlInfo.Length > 0)
        //    {
        //        return controller.ControlInfo
        //            .SelectMany(info =>
        //            {
        //                if(info.LightControlOptions.Length > 0)
        //                {
        //                    var final = new List<LightControlOption>();

        //                    var nonStaged = info.LightControlOptions
        //                        .Where(option => !(option is StagedControlOption));

        //                    final.AddRange(nonStaged);

        //                    var staged = info.LightControlOptions
        //                        .OfType<StagedControlOption>();

        //                    foreach(StagedControlOption stagedControl in staged)
        //                    {
        //                        final.AddRange(GetLastRecursive(stagedControl));
        //                    }

        //                    return final;
        //                }
        //                else
        //                {
        //                    return new List<LightControlOption>();
        //                }
        //            });
        //    }
        //    else
        //    {
        //        return new LightControlOption[0];
        //    }
        //}

        private IEnumerable<LightControlOption> GetLastRecursive(StagedControlOption stagedOption)
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
                                .OfType<StagedControlOption>(); //fix this

                            foreach (StagedControlOption stagedControl in staged)
                            {
                                final.AddRange(GetLastRecursive(stagedControl));
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

        private bool HasClampedIteration(LightControlOption option)
        {
            if(option is IntensityControlOption)
            {
                IntensityControlOption casted = (IntensityControlOption)option;

                if(casted.CurvedIntensities.UseControl)
                {
                    if(casted.CurvedIntensities.MinMaxCurve.mode == ParticleSystemCurveMode.Curve)
                    {
                        return casted.CurvedIntensities.MaxCurveTracker.WrapMode == MinMaxWrapMode.Clamp;
                    }
                    else if(casted.CurvedIntensities.MinMaxCurve.mode == ParticleSystemCurveMode.TwoCurves)
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
            else if(option is ColorControlOption)
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

        //private void Start()
        //{
        //    //var test = GetRecursive(this).ToList();

        //    //test.ForEach(control => control.)

        //    //Debug.Assert(test.All(control => test.Count(tested => tested == control) == 1));

        //    //SetRecursiveStack(this);
        //}

        //private IEnumerable<LightControlOption> GetRecursive(StagedLightController controller)
        //{
        //    return controller
        //        .ControllerStages.Stages
        //        //.SelectMany(stage => stage.Stages)
        //        .SelectMany(stage => stage.LightControllers)
        //        .SelectMany(control =>
        //        {
        //            if (control is StagedLightController)
        //                return GetRecursive((StagedLightController)control);
        //            else
        //                return ((GroupedLightController)control)
        //                    .ControlInfo
        //                    .SelectMany(info => info.LightControlOptions);
        //        });
        //}

        //private Stack<GroupedLightController> GetRecursiveTest(StagedLightController controller)
        //{
        //    var stack = new Stack<StagedLightController>();
        //    var stack2 = new Stack<GroupedLightController>();

        //    controller
        //        .ControllerStages.Stages
        //        .SelectMany(stage => stage.LightControllers)
        //        .ToList().ForEach(control =>
        //        {
        //            if (control is StagedLightController)
        //            {
        //                stack.Push((StagedLightController)control);
        //            }
        //            else
        //            {
        //                stack2.Push((GroupedLightController)control);
        //            }
        //        });

        //    var found = new Stack<StagedLightController>();

        //    stack.ToList().ForEach(item => found.Push(item));

        //    while(found.Any())
        //    {
        //        found.ToList().ForEach(item => stack.Push(item));

        //        var temp = new Stack<StagedLightController>();

        //        found.ToList().ForEach(item => temp.Push(item));

        //        found = new Stack<StagedLightController>();

        //        temp.ToList().ForEach(item =>
        //        {
        //            item.ControllerStages.Stages
        //                .SelectMany(stage => stage.LightControllers)
        //                .ToList().ForEach(control =>
        //                {
        //                    if (control is StagedLightController)
        //                    {
        //                        found.Push((StagedLightController)control);
        //                    }
        //                    else
        //                    {
        //                        stack2.Push((GroupedLightController)control);
        //                    }
        //                });
        //        });
        //    }

        //    return stack2;
        //}

        //private Stack<InstancedControlOption> GetRecursiveTest2(StagedLightController controller)
        //{
        //    var stack = GetRecursiveTest(controller);
        //    var stack2 = new Stack<InstancedControlOption>();

        //    while (stack.Any())
        //    {
        //        GroupedLightController current = stack.Pop();

        //        current.ControlInfo
        //            .SelectMany(info => info.LightControlOptions)
        //            .ToList().ForEach(option =>
        //            {
        //                stack2.Push(option);

        //                var options = new List<StagedControlOption>();

        //                if (option.ClonedOption is StagedControlOption)
        //                {
        //                    var original = ReflectionUtils.GetMemberAtPath<StagedControlOption>(option, "originalOption");
        //                    var cloned = ReflectionUtils.GetMemberAtPath<StagedControlOption>(option, "originalOption");

        //                    options.Add(original);
        //                    options.Add(cloned);
        //                }

        //                while (options.FirstOrDefault() != null)
        //                {
        //                    var first = options.First();

        //                    first.Stager.Stages
        //                        .SelectMany(stage => stage.ControlOptions)
        //                        .ToList().ForEach(stage =>
        //                        {
        //                            stack2.Push(stage);
        //                        });

        //                    options.Remove(first);
        //                    //add more here
        //                }
        //            });
        //    }

        //    return stack2;
        //}

        //private void SetRecursiveStack(StagedLightController controller)
        //{
        //    var stack = GetRecursiveTest2(controller);

        //    while(stack.Any())
        //    {
        //        InstancedControlOption current = stack.Pop();

        //        ReflectionUtils.SetMemberAtPath(current, UnityEngine.Object.Instantiate<LightControlOption>(ReflectionUtils.GetMemberAtPath<LightControlOption>(current, "originalOption")), "originalOption");
        //        ReflectionUtils.SetMemberAtPath(current, UnityEngine.Object.Instantiate<LightControlOption>(ReflectionUtils.GetMemberAtPath<LightControlOption>(current, "clonedOption")), "clonedOption");
        //    }

        //    //var current = controller
        //    //    .ControllerStages.Stages
        //    //    //.SelectMany(stage => stage.Stages)
        //    //    .SelectMany(stage => stage.LightControllers);

        //    //    current.ToList().ForEach()
        //    //    .SelectMany(control =>
        //    //    {
        //    //        if (control is StagedLightController)
        //    //            return GetRecursive((StagedLightController)control);
        //    //        else
        //    //            return ((GroupedLightController)control)
        //    //                .ControlInfo
        //    //                .SelectMany(info => info.LightControlOptions);
        //    //    });
        //}

        //private void SetRecursive(InstancedControlOption option)
        //{
        //    LightControlOption originalToSet = UnityEngine.Object.Instantiate<LightControlOption>(ReflectionUtils.GetMemberAtPath<LightControlOption>(option, "originalOption"));
        //    LightControlOption clonedToSet = UnityEngine.Object.Instantiate<LightControlOption>(ReflectionUtils.GetMemberAtPath<LightControlOption>(option, "clonedOption"));

        //    ReflectionUtils.SetMemberAtPath(option, originalToSet, "originalOption");
        //    ReflectionUtils.SetMemberAtPath(option, clonedToSet, "clonedOption");
        //}
    }
}