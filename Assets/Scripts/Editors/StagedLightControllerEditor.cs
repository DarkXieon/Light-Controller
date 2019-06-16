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
        private static readonly string[] _dontIncludeMe = new string[] { "m_Script" };

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, _dontIncludeMe);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
