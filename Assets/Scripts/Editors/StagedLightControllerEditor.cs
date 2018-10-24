#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using LightControls.Controllers;

using UnityEditor;

namespace LightControls.Editors
{
    [InitializeOnLoad]
    [CustomEditor(typeof(StagedLightController))]
    public class StagedLightControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}

#endif
