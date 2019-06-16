#if UNITY_EDITOR

using System.Collections.Generic;

using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(LightControllerStager))]
    public class LightControllerStagerPropertyDrawer : StagerPropertyDrawer
    {
        private class PropertyData
        {
            public StagerReorderableListWrapper DisplayList;
            public SerializedProperty Stages;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            base.OnGUI(position, property, label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(position, label, property);

            PropertyDrawerRect currentRect = new PropertyDrawerRect(position);

            Rect temp = currentRect.CurrentRect;
            temp.y += base.GetPropertyHeight(property, label);
            currentRect.CurrentRect = temp;

            using (EditorUtils.StartIndent(currentRect, indent))
            {   
                currentProperty.DisplayList.DisplayList(currentRect.CurrentRect);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            return base.GetPropertyHeight(property, label) + currentProperty.DisplayList.ListHeight;
        }

        private void Initialize(SerializedProperty property)
        {
            string key = property.serializedObject.targetObject.GetInstanceID().ToString();
            
            if (!currentPropertyPerPath.TryGetValue(key, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    DisplayList = new StagerReorderableListWrapper(property.FindPropertyRelative("stages"), UpdateInstancedAdd, UpdateInstancedRemove, UpdateInstancedReorder),
                    Stages = property.FindPropertyRelative("stages")
                };

                currentPropertyPerPath.Add(key, currentProperty);
            }
        }
        
        private void UpdateInstancedAdd(SerializedProperty property)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<UnityEngine.Object>(
                property: property,
                containers: property.serializedObject.targetObjects,
                updateOperation: target => StagerReorderableListWrapper.DefaultUpdateInstancedAdd<LightControllerStage, InstancedLightControllerStage>(
                    dataParentContainer: target,
                    instancedParentContainer: target,
                    dataMemberPath: "controllerStager.stages",
                    instancedMemberPath: "instancedStager.controllerStages",
                    iterationsMemberPath: "instancedStager.iterations",
                    stage => new InstancedLightControllerStage(stage)));
        }

        private void UpdateInstancedRemove(SerializedProperty property, int removedAt)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<UnityEngine.Object>(
                property: property,
                containers: property.serializedObject.targetObjects,
                updateOperation: target => StagerReorderableListWrapper.DefaultInstancedRemove<ControlOptionStage, InstancedControlOptionStage>(
                    dataParentContainer: target,
                    instancedParentContainer: target,
                    removedAt: removedAt,
                    dataMemberPath: "controllerStager.stages",
                    instancedMemberPath: "instancedStager.controllerStages",
                    iterationsMemberPath: "instancedStager.iterations"));
        }


        private void UpdateInstancedReorder(SerializedProperty property, int oldIndex, int newIndex)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<UnityEngine.Object>(
                property: property,
                containers: property.serializedObject.targetObjects,
                updateOperation: target => StagerReorderableListWrapper.DefaultInstancedReorder<ControlOptionStage, InstancedControlOptionStage>(
                    dataParentContainer: target,
                    instancedParentContainer: target,
                    oldIndex: oldIndex,
                    newIndex: newIndex,
                    dataMemberPath: "controllerStager.stages",
                    instancedMemberPath: "instancedStager.controllerStages",
                    iterationsMemberPath: "instancedStager.iterations"));
        }
    }
}

#endif