#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using LightControls.ControlOptions;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;
using static LightControls.Utilities.EditorUtils;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(ControlOptionStager))]
    public class ControlOptionStagerPropertyDrawer : StagerPropertyDrawer
    {
        private class PropertyData
        {
            public StagerReorderableListWrapper DisplayList;
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
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    DisplayList = new StagerReorderableListWrapper(
                        property.FindPropertyRelative("stages"),
                        UpdateInstancedAdd,
                        UpdateInstancedRemove,
                        UpdateInstancedReorder)
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }

        private void UpdateInstancedAdd(SerializedProperty list)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<FoundControl>(
                property: list,
                containers: EditorUtils.FindControls<StagedControlOption>(list),
                updateOperation: foundControl => StagerReorderableListWrapper.DefaultUpdateInstancedAdd<ControlOptionStage, InstancedControlOptionStage>(
                    dataParentContainer: foundControl.ControlOption,
                    instancedParentContainer: foundControl.InstancedOption,
                    dataMemberPath: "stager.stages",
                    instancedMemberPath: "instancedOptionStager.instancedStages",
                    iterationsMemberPath: "instancedOptionStager.iterations",
                    stage => new InstancedControlOptionStage(stage)));
        }

        private void UpdateInstancedRemove(SerializedProperty list, int removedAt)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<FoundControl>(
                property: list,
                containers: EditorUtils.FindControls<StagedControlOption>(list),
                updateOperation: foundControl => StagerReorderableListWrapper.DefaultInstancedRemove<ControlOptionStage, InstancedControlOptionStage>(
                    dataParentContainer: foundControl.ControlOption,
                    instancedParentContainer: foundControl.InstancedOption,
                    removedAt: removedAt,
                    dataMemberPath: "stager.stages",
                    instancedMemberPath: "instancedOptionStager.instancedStages",
                    iterationsMemberPath: "instancedOptionStager.iterations"));
        }


        private void UpdateInstancedReorder(SerializedProperty list, int oldIndex, int newIndex)
        {
            StagerReorderableListWrapper.DefaultInstancedUpdate<FoundControl>(
                property: list,
                containers: EditorUtils.FindControls<StagedControlOption>(list),
                updateOperation: foundControl => StagerReorderableListWrapper.DefaultInstancedReorder<ControlOptionStage, InstancedControlOptionStage>(
                    dataParentContainer: foundControl.ControlOption,
                    instancedParentContainer: foundControl.InstancedOption,
                    oldIndex: oldIndex,
                    newIndex: newIndex,
                    dataMemberPath: "stager.stages",
                    instancedMemberPath: "instancedOptionStager.instancedStages",
                    iterationsMemberPath: "instancedOptionStager.iterations"));
        }
    }
}

#endif