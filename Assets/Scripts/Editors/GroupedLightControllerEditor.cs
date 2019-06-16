#if UNITY_EDITOR

using System.Collections.Generic;
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
        public const string groupsDataPath = "lightControllerGroupsData";
        public const string groupsInstancedPath = "lightControllerGroups";
        public const string groupsIterationsPath = "controlIterations";

        private static readonly GUIContent lightControlGroupsContent;
        private static readonly GUIContent lightControlGroupElementContent;

        static GroupedLightControllerEditor()
        {
            lightControlGroupsContent = new GUIContent("Light Control Groups", "");
            lightControlGroupElementContent = new GUIContent("Group", "");
        }

        private class PropertyData
        {
            public StagerReorderableListWrapper DisplayList;
            public SerializedProperty Groups;
        }
        
        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnInspectorGUI()
        {
            Initialize();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            LightControllerGroupData[] groupsData = ReflectionUtils.GetMemberAtPath<LightControllerGroupData[]>(target, "lightControllerGroupsData");

            int size = EditorUtils.ArraySizeField(lightControlGroupsContent, groupsData.Length);
            MiscUtils.ResizeAndFill(ref groupsData, size);

            ReflectionUtils.SetMemberAtPath(target, groupsData, "lightControllerGroupsData");
            serializedObject.Update();

            Rect listRect = currentProperty.Groups.arraySize > 0
                ? EditorGUILayout.GetControlRect(false, currentProperty.DisplayList.ListHeight)
                : EditorGUILayout.GetControlRect(false, currentProperty.DisplayList.ListHeight + 20f);
            
            currentProperty.DisplayList.DisplayList(listRect);

            serializedObject.ApplyModifiedProperties();
        }
        
        private void Initialize()
        {
            string key = serializedObject.targetObject.GetInstanceID().ToString();
            
            if (!currentPropertyPerPath.TryGetValue(key, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    DisplayList = new StagerReorderableListWrapper(serializedObject.FindProperty(groupsDataPath), UpdateInstancedAdd, UpdateInstancedRemove, UpdateInstancedReorder),
                    Groups = serializedObject.FindProperty(groupsDataPath)
                };

                currentPropertyPerPath.Add(key, currentProperty);
            }
        }
        
        private void UpdateInstancedAdd(SerializedProperty property)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<UnityEngine.Object>(
                property: property,
                containers: property.serializedObject.targetObjects,
                updateOperation: controller => StagerReorderableListWrapper.DefaultUpdateInstancedAdd<LightControllerGroupData, LightControllerGroup>(
                    dataParentContainer: controller,
                    instancedParentContainer: controller,
                    dataMemberPath: groupsDataPath,
                    instancedMemberPath: groupsInstancedPath,
                    iterationsMemberPath: groupsIterationsPath,
                    stage => new LightControllerGroup(stage)));
        }

        private void UpdateInstancedRemove(SerializedProperty property, int removedAt)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<UnityEngine.Object>(
                property: property,
                containers: property.serializedObject.targetObjects,
                updateOperation: controller => StagerReorderableListWrapper.DefaultInstancedRemove<LightControllerGroupData, LightControllerGroup>(
                    dataParentContainer: controller,
                    instancedParentContainer: controller,
                    dataMemberPath: groupsDataPath,
                    instancedMemberPath: groupsInstancedPath,
                    iterationsMemberPath: groupsIterationsPath,
                    removedAt: removedAt));
        }


        private void UpdateInstancedReorder(SerializedProperty property, int oldIndex, int newIndex)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<UnityEngine.Object>(
                property: property,
                containers: property.serializedObject.targetObjects,
                updateOperation: controller => StagerReorderableListWrapper.DefaultInstancedReorder<LightControllerGroupData, LightControllerGroup>(
                    dataParentContainer: controller,
                    instancedParentContainer: controller,
                    dataMemberPath: groupsDataPath,
                    instancedMemberPath: groupsInstancedPath,
                    iterationsMemberPath: groupsIterationsPath,
                    oldIndex: oldIndex,
                    newIndex: newIndex));
        }
    }
}

#endif