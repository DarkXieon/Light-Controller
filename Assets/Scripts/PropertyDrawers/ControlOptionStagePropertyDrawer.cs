#if UNITY_EDITOR

using System;
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
    [CustomPropertyDrawer(typeof(ControlOptionStage))]
    public class ControlOptionStagePropertyDrawer : StagePropertyDrawer
    {
        private static readonly GUIContent stageControlsAmountContent;
        private static readonly GUIContent stageControlsElementContent;

        static ControlOptionStagePropertyDrawer()
        {
            stageControlsAmountContent = new GUIContent("Stage Controls", "");
            stageControlsElementContent = new GUIContent("Stage Control", "");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            SerializedProperty controlOptions = property.FindPropertyRelative("controlOptions");

            int indent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            indentedPosition.y += base.GetPropertyHeight(property, label);

            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();

            int size = EditorUtils.ArraySizeField(indentedPosition, stageControlsAmountContent, controlOptions.arraySize, controlOptions.hasMultipleDifferentValues);

            if (EditorGUI.EndChangeCheck())
            {
                LightControlOption[] currentOptions = controlOptions.GetArray<LightControlOption>();
                currentOptions = MiscUtils.ResizeAndFillWith(currentOptions, size, () => null);
                controlOptions.SetArray(currentOptions);
            }

            EditorUtils.ArraySizeField(indentedPosition, controlOptions, stageControlsAmountContent);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(controlOptions, stageControlsElementContent));
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, controlOptions, stageControlsElementContent);

            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                UpdateInstancedControls(property);
            }

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty controlOptions = property.FindPropertyRelative("controlOptions");

            float controlsHeight = EditorUtils.LineHeight + EditorUtils.VerticalBuffer + EditorUtils.GetSinglelinePropertyArrayHeight(controlOptions, stageControlsElementContent);

            return base.GetPropertyHeight(property, label) + controlsHeight + EditorUtils.VerticalBuffer;
        }

        protected override string GetInstancedPath(SerializedProperty self)
        {
            string[] splitOriginalPath = self.propertyPath.Split('.');
            string instancedStagePath = $"instancedOptionStager.instancedStages.Array.{splitOriginalPath[splitOriginalPath.Length - 1]}";


            return instancedStagePath;
        }

        private void UpdateInstancedControls(SerializedProperty self)
        {
            self.serializedObject.ApplyModifiedProperties();

            FoundControl[] foundControls = EditorUtils.FindControls<StagedControlOption>(self);

            foreach(FoundControl found in foundControls)
            {
                string controlPropertyPath = $"{found.OptionPath}.{self.propertyPath}";
                string instancedPropertyPath = $"{found.InstancedPath}.{self.propertyPath}".Replace("stager", "instancedOptionStager").Replace("stages", "instancedStages");

                string serializedOptionsPropertyPath = $"{controlPropertyPath}.controlOptions";
                string instancedOptionsPropertyPath = $"{instancedPropertyPath}.controlOptions";
                string iterationsPropertyPath = $"{instancedPropertyPath}.iterations";
                
                string[] controlPathSplit = found.InstancedPath.Split('.');
                string updatePropertyPath = string.Join(".", controlPathSplit.Take(controlPathSplit.Length - 3));
                string updateTogglePath = $"{updatePropertyPath}.controlOptionGroup.UpdateColorInfo";
                
                EditorUtils.UpdateInstancedArray<LightControlOption, InstancedControlOption>(
                    serializedContainer: found.Controller,
                    instancedContainer: found.Controller,
                    serializedPath: serializedOptionsPropertyPath,
                    instancedPath: instancedOptionsPropertyPath,
                    iterationsPath: iterationsPropertyPath,
                    isInstancedVersion: (option, instanced) => EditorUtils.InstancedVersionOf(instanced, option),
                    getInstancedVersion: option => option.GetInstanced());

                ReflectionUtils.SetMemberAtPath(found.Controller, true, updateTogglePath);
            }
        }
    }
}

#endif