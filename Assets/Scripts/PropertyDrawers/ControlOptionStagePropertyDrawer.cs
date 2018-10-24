﻿#if UNITY_EDITOR

using LightControls.ControlOptions;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

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

            SerializedProperty controlOptions = property.FindPropertyRelative("ControlOptions");

            int indent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            indentedPosition.y += base.GetPropertyHeight(property, label);

            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            int size = EditorUtils.ArraySizeField(indentedPosition, stageControlsAmountContent, controlOptions.arraySize, controlOptions.hasMultipleDifferentValues);
            
            if (EditorGUI.EndChangeCheck())
            {
                LightControlOption[] currentOptions = controlOptions.GetArray<LightControlOption>();
                currentOptions = MiscUtils.ResizeAndFillWith(currentOptions, size, () => null);
                controlOptions.SetArray(currentOptions);
            }

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(controlOptions, stageControlsElementContent));
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, controlOptions, stageControlsElementContent);

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty controlOptions = property.FindPropertyRelative("ControlOptions");

            float controlsHeight = EditorUtils.LineHeight + EditorUtils.VerticalBuffer + EditorUtils.GetSinglelinePropertyArrayHeight(controlOptions, stageControlsElementContent);

            return base.GetPropertyHeight(property, label) + controlsHeight + EditorUtils.VerticalBuffer;
        }
    }
}

#endif