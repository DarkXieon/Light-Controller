#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using LightControls.ControlOptions.ControlGroups;
using LightControls.ControlOptions.ControlGroups.Data;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(ListControlGroupData))]
    public class ListControlGroupDrawer : PropertyDrawer
    {
        private static readonly GUIContent randomizeBetweenIntensityValuesContent;
        private static readonly GUIContent randomizeIntensityEntryIndexContent;
        private static readonly GUIContent intensityListLengthContent;
        private static readonly GUIContent intensityListElementContent;

        static ListControlGroupDrawer()
        {
            randomizeBetweenIntensityValuesContent = new GUIContent("Randomize Between List Entries", "This will cause each new transition value to be randomized between the previous list entry and the next one. For example if you have the values 5, 15, and 10, rather than going to 5, then 15, then 10, it will go from 5, to a number between 5 and 15, to a number between 15 and 10.");
            randomizeIntensityEntryIndexContent = new GUIContent("Randomize Chosen List Entry", "This will cause each new intensity target to be randomly chosen from the list rather than being chosen in order. For example if you have the values 5, 15, and 10, the intensity might go 5, 15, 5, 15, 10, and 15 rather than looping through in order.");
            intensityListLengthContent = new GUIContent("Number of Intensity Values", "This is the number of values that you'd like to be in the list");
            intensityListElementContent = new GUIContent("Intensity");
        }

        private class PropertyData
        {
            public SerializedProperty RandomizeBetweenValues;
            public SerializedProperty RandomizeChosenEntry;
            public SerializedProperty ListedValues;
            public SerializedProperty MaxValue;
            public SerializedProperty MinValue;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int indent = EditorGUI.indentLevel;

            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;
            Rect current = EditorUtils.GetTopRect(indentedPosition, EditorUtils.LineHeight);
            
            currentProperty.RandomizeBetweenValues.boolValue = EditorGUI.Toggle(current, randomizeBetweenIntensityValuesContent, currentProperty.RandomizeBetweenValues.boolValue);
            current = EditorUtils.GetRectBelow(current, EditorUtils.LineHeight);
            currentProperty.RandomizeChosenEntry.boolValue = EditorGUI.Toggle(current, randomizeIntensityEntryIndexContent, currentProperty.RandomizeChosenEntry.boolValue);
            current = EditorUtils.GetRectBelow(current, EditorUtils.LineHeight);
            current = Rect.MinMaxRect(current.xMin, current.yMin, current.xMax, position.yMax);
            currentProperty.ListedValues.SetArray(EditorUtils.FloatArrayField(current, intensityListLengthContent, intensityListElementContent, currentProperty.ListedValues.GetArray<float>()));
            CalculateMinMaxs();

            EditorGUI.EndProperty();

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            return (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * (3 + currentProperty.ListedValues.GetArray<float>().Length);
        }


        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    RandomizeBetweenValues = property.FindPropertyRelative("randomizeBetweenValues"),
                    RandomizeChosenEntry = property.FindPropertyRelative("randomizeChosenEntry"),
                    ListedValues = property.FindPropertyRelative("listedValues"),
                    MaxValue = property.FindPropertyRelative("maxValue"),
                    MinValue = property.FindPropertyRelative("minValue")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }

        private void CalculateMinMaxs()
        {
            float[] listedValues = currentProperty.ListedValues.GetArray<float>();

            if(listedValues.Length > 0)
            {
                currentProperty.MinValue.floatValue = listedValues.Min();
                currentProperty.MaxValue.floatValue = listedValues.Max();
            }
        }
    }
}

#endif