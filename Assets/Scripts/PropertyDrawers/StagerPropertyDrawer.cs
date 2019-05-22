#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Stager))]
    public class StagerPropertyDrawer : PropertyDrawer
    {
        private readonly GUIContent useStagesContent = new GUIContent("Use Stages", "");
        private readonly GUIContent randomizeStageOrderContent = new GUIContent("Randomize Stage Order", "");
        private readonly GUIContent allowLooseOrderContent = new GUIContent("Allow Loose Order", "");

        private readonly GUIContent controlOptionsLengthContent = new GUIContent("Control Options", "");
        private readonly GUIContent controlOptionsElementContent = new GUIContent("Control Option", "");

        private class PropertyData
        {
            public SerializedProperty RandomizeNextStage;
            public SerializedProperty CanSkipStages;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedRect = EditorUtils.BeginPropertyDrawer(position);

            indentedRect = EditorUtils.GetTopRect(indentedRect, EditorStyles.label);
            currentProperty.RandomizeNextStage.boolValue = EditorGUI.Toggle(indentedRect, randomizeStageOrderContent, currentProperty.RandomizeNextStage.boolValue);

            indentedRect = EditorUtils.GetRectBelow(indentedRect, EditorStyles.label);
            currentProperty.CanSkipStages.boolValue = EditorGUI.Toggle(indentedRect, allowLooseOrderContent, currentProperty.CanSkipStages.boolValue);
            
            EditorUtils.EndPropertyDrawer();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            return (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f;
        }

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    RandomizeNextStage = property.FindPropertyRelative("randomizeNextStage"),
                    CanSkipStages = property.FindPropertyRelative("canSkipStages")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }

        protected static void UpdateInstancedBase<TInfo>(SerializedProperty property, TInfo[] info, Action<TInfo> instancedOperation)
        {
            property.serializedObject.ApplyModifiedProperties();

            for (int i = 0; i < info.Length; i++)
            {
                instancedOperation(info[i]);
            }

            property.serializedObject.Update();
        }

    }
}

#endif