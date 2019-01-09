#if UNITY_EDITOR

using System.Collections.Generic;
using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(AudioIntensityGenerator))]
    public class AudioIntensityGeneratorPropertyDrawer : PropertyDrawer
    {
        private static GUIContent hasAuthorityContent;
        private static GUIContent minMaxLabelContent;
        private static GUIContent minIntensityContent;
        private static GUIContent maxIntensityContent;
        private static GUIContent controlTargetContent;

        static AudioIntensityGeneratorPropertyDrawer()
        {
            hasAuthorityContent = new GUIContent("Generate Intensity From Audio", "");
            minMaxLabelContent = new GUIContent("Intensity Limits", "");
            minIntensityContent = new GUIContent("Min Intensity: ", "");
            maxIntensityContent = new GUIContent("Max Intensity: ", "");
            controlTargetContent = new GUIContent("Affecting Properties", "");
        }

        private class PropertyData
        {
            public SerializedProperty HasAuthorityProperty;
            public SerializedProperty MinIntensityProperty;
            public SerializedProperty MaxIntensityProperty;
            public SerializedProperty ControlTargetProperty;
            public SerializedProperty IntensityModifiers;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            int indent = EditorGUI.indentLevel;
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            float previousFieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.label);

            currentProperty.HasAuthorityProperty.boolValue = EditorGUI.Toggle(indentedPosition, hasAuthorityContent, currentProperty.HasAuthorityProperty.boolValue);

            if(currentProperty.HasAuthorityProperty.boolValue)
            {
                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.label);

                Rect[] splitLine = EditorUtils.SplitLineRect(
                    lineRect: indentedPosition,
                    labelContent: new GUIContent[] { minMaxLabelContent, minIntensityContent, maxIntensityContent },
                    sizePercentages: new float[] { .4f, .30f, .30f },
                    labelStyle: EditorStyles.label,
                    linesDownward: 0);

                EditorGUI.LabelField(splitLine[0], minMaxLabelContent);

                EditorGUI.LabelField(splitLine[2], minIntensityContent);
                currentProperty.MinIntensityProperty.floatValue = EditorGUI.FloatField(splitLine[3], currentProperty.MinIntensityProperty.floatValue);

                EditorGUI.LabelField(splitLine[4], maxIntensityContent);
                currentProperty.MaxIntensityProperty.floatValue = EditorGUI.FloatField(splitLine[5], currentProperty.MaxIntensityProperty.floatValue);

                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.label);
                currentProperty.ControlTargetProperty.intValue = (int)(IntensityControlTarget)EditorGUI.EnumFlagsField(indentedPosition, controlTargetContent, (IntensityControlTarget)currentProperty.ControlTargetProperty.intValue);

                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetIntensityModifiersHeight(currentProperty.ControlTargetProperty));
                EditorUtils.DisplayIntensityModifiers(indentedPosition, currentProperty.ControlTargetProperty, currentProperty.IntensityModifiers);
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUIUtility.fieldWidth = previousFieldWidth;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            float afterToggle = currentProperty.HasAuthorityProperty.boolValue
                ? EditorUtils.LineHeight * 2 + EditorUtils.GetIntensityModifiersHeight(currentProperty.ControlTargetProperty) + EditorUtils.VerticalBuffer * 3
                : 0f;

            return EditorUtils.LineHeight + EditorUtils.VerticalBuffer + afterToggle;// * EditorGUIUtility.pixelsPerPoint * 4;
        }

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    HasAuthorityProperty = property.FindPropertyRelative("hasAuthority"),
                    MinIntensityProperty = property.FindPropertyRelative("minIntensity"),
                    MaxIntensityProperty = property.FindPropertyRelative("maxIntensity"),
                    ControlTargetProperty = property.FindPropertyRelative("controlTarget"),
                    IntensityModifiers = property.FindPropertyRelative("intensityModifers")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }
    }
}

#endif