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

#if UNITY_EDITOR
            WarningSystem.DisplayStagedLightControllerWarnings(this);
#endif
        }

        public void UpdateInstancedController()
        {
            instancedStager.UpdateInstanced();
        }
    }
}