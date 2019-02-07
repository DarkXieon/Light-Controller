#if UNITY_EDITOR

using System.Linq;
using LightControls.Controllers;
using LightControls.Controllers.Data;
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

            LightControllerGroupData[] groupsData = ReflectionUtils.GetMemberAtPath<LightControllerGroupData[]>(target, "lightControllerGroupsData");

            int size = EditorUtils.ArraySizeField(lightControlGroupsContent, groupsData.Length);
            MiscUtils.ResizeAndFill(ref groupsData, size);

            ReflectionUtils.SetMemberAtPath(target, groupsData, "lightControllerGroupsData");
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            SerializedProperty groupsDataProperty = serializedObject.FindProperty("lightControllerGroupsData");
            EditorUtils.DisplayMultilinePropertyArray(groupsDataProperty, lightControlGroupElementContent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif