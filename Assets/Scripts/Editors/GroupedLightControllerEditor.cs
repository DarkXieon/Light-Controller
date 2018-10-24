#if UNITY_EDITOR

using System.Linq;
using LightControls.Controllers;
using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.Editors
{
    [InitializeOnLoad]
    [CustomEditor(typeof(GroupedLightController))]
    public class GroupedLightControllerEditor : Editor
    {
        private static readonly GUIContent lightControlGroupsContent;
        private static readonly GUIContent lightControlGroupElementContent;

        static GroupedLightControllerEditor()
        {
            lightControlGroupsContent = new GUIContent("Light Control Groups", "");
            lightControlGroupElementContent = new GUIContent("Group", "");
        }

        public override void OnInspectorGUI()
        {
            var controller = target as GroupedLightController;
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            int size = EditorUtils.ArraySizeField(lightControlGroupsContent, controller.ControlInfo.Length);
            
            controller.ControlInfo = MiscUtils.ResizeAndFill(controller.ControlInfo, size);
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorUtils.DisplayMultilinePropertyArray(serializedObject.FindProperty("ControlInfo"), lightControlGroupElementContent);
        }
    }
}

#endif