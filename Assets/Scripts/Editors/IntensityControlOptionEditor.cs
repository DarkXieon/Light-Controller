#if UNITY_EDITOR

using System;
using System.Linq;

using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.Editors
{
    [InitializeOnLoad]
    [CanEditMultipleObjects]
    [CustomEditor(typeof(IntensityControlOption))]
    public class IntensityControlOptionEditor : Editor
    {
        private static readonly GUIContent mainLabelContent;
        private static readonly GUIContent useIntensityControlsContent;

        private static readonly GUIContent intensityGenerationLabel;
        private static readonly GUIContent intensityTargetLabel;
        private static readonly GUIContent intensityGenerationModeLabelContent;

        private static readonly GUIContent rateOfChangeContent;
        private static readonly GUIContent roCGenerationModeLabelContent;
        private static readonly GUIContent roCMode;

        private static readonly GUIContent lightIntensityControlTargetContent;
        private static readonly GUIContent lightColorIntensityControlTargetContent;
        private static readonly GUIContent materialColorIntensityControlTargetContent;
        private static readonly GUIContent lightRangeControlTargetContent;
        private static readonly GUIContent spotlightAngleControlTargetContent;
        private static readonly GUIContent scaleContent;
        private static readonly GUIContent offsetContent;
        private static readonly GUIContent relateInverslyContent;

        private static readonly GUIContent miscellaneousContent;

        private const float fieldMinSize = 50f;
        
        static IntensityControlOptionEditor()
        {
            mainLabelContent = new GUIContent(EditorUtils.GetRichText("Intensity Control Options", 12, EditorUtils.TextOptions.Bold), "These controls allow you to set how you would like the light controller to handle light intensity");
            useIntensityControlsContent = new GUIContent("Use Intensity Controls", "If enabled, the intensity of the lights will changed based on your selections below");

            intensityGenerationLabel = new GUIContent(EditorUtils.GetRichText("Intensity Generation", EditorUtils.TextOptions.Bold), "These options affect how the intensity values that are being toggled between are found/generated");
            intensityTargetLabel = new GUIContent("Affecting Properties");
            intensityGenerationModeLabelContent = new GUIContent("Generation Mode");

            rateOfChangeContent = new GUIContent(EditorUtils.GetRichText("Rate of Change (RoC) Generation", EditorUtils.TextOptions.Bold), "These options affect how the rate of change is calculated. Rate of change affects how quickly the lights transition from one intensity to another.");
            roCGenerationModeLabelContent = new GUIContent("Generation Mode");
            roCMode = new GUIContent("RoC Mode");

            lightIntensityControlTargetContent = new GUIContent("Light Intensity:", "");
            lightColorIntensityControlTargetContent = new GUIContent("Light Color Intensity:", "");
            materialColorIntensityControlTargetContent = new GUIContent("Light Material Intensity:", "");
            lightRangeControlTargetContent = new GUIContent("Light Range:", "");
            spotlightAngleControlTargetContent = new GUIContent("Spotlight Angle:", "");
            scaleContent = new GUIContent("Scale: ");
            offsetContent = new GUIContent("Offset: ");
            relateInverslyContent = new GUIContent("Relate Inversely: ");

            miscellaneousContent = new GUIContent(EditorUtils.GetRichText("Miscellaneous", EditorUtils.TextOptions.Bold), "These options are other useful utilities that can be used to further customize light intensity display.");
        }

        private class PropertyData
        {
            public SerializedProperty ControlTargetProperty;
            public SerializedProperty CurvedIntensitiesProperty;
            public SerializedProperty ListedIntensitiesProperty;
            public SerializedProperty CurvedIntensitiesUseControlProperty;
            public SerializedProperty ListedIntensitiesUseControlProperty;
            public SerializedProperty RocModeProperty;
            public SerializedProperty CurvedRoCProperty;
            public SerializedProperty ListedRoCProperty;
            public SerializedProperty CurvedRoCUseControlProperty;
            public SerializedProperty ListedRoCUseControlProperty;
            public SerializedProperty IntensityModifersProperty;
            public SerializedProperty NoSmoothTransitionsProperty;
            public SerializedProperty UseLightIntensityFloorProperty;
            public SerializedProperty UseLightIntensityCeilingProperty;
            public SerializedProperty FloorProperty;
            public SerializedProperty CeilingProperty;
        }

        private PropertyData currentProperty;
        private Vector2 scrollPosition;
        
        public override void OnInspectorGUI()
        {
            Initialize(serializedObject);
            
            EditorGUIUtility.labelWidth = 250f;
            
            if(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth < 350f)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            }
            
            EditorGUILayout.LabelField(mainLabelContent, EditorUtils.RichTextStyle);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField(intensityGenerationLabel, EditorUtils.RichTextStyle);
            
            currentProperty.ControlTargetProperty.intValue = (int)(IntensityControlTarget)EditorGUILayout.EnumFlagsField(intensityTargetLabel, (IntensityControlTarget)currentProperty.ControlTargetProperty.intValue);
            
            EditorGUI.BeginChangeCheck();

            ValueGenerationMode intensityGenerationMode = (ValueGenerationMode)EditorGUILayout.EnumPopup(intensityGenerationModeLabelContent, EditorUtils.GetValueGenerationMode(currentProperty.CurvedIntensitiesProperty, currentProperty.ListedIntensitiesProperty));

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtils.SetValueGenerationMode(currentProperty.CurvedIntensitiesProperty, currentProperty.ListedIntensitiesProperty, intensityGenerationMode);
            }

            if (currentProperty.CurvedIntensitiesUseControlProperty.boolValue)
            {
                EditorGUILayout.PropertyField(currentProperty.CurvedIntensitiesProperty);
            }
            else if (currentProperty.ListedIntensitiesUseControlProperty.boolValue)
            {
                EditorGUILayout.PropertyField(currentProperty.ListedIntensitiesProperty);
            }
            
            string currentBinaryString = Convert.ToString(currentProperty.ControlTargetProperty.intValue, 2);

            Debug.Assert(currentBinaryString.Count() <= currentProperty.IntensityModifersProperty.arraySize || currentBinaryString.Count() == Convert.ToString(~0, 2).Count());
            
            for (int i = 0; i < currentBinaryString.Length && i < currentProperty.IntensityModifersProperty.arraySize; i++) // the only time the second condition will stop the loop is when currentProperty.ControlTargetProperty is set to IntensityControlTarget.Everything
            {
                if (currentBinaryString[currentBinaryString.Length - 1 - i] == '1') //The binary number will have the lowest values on the right and not the left
                {
                    SerializedProperty element = currentProperty.IntensityModifersProperty.GetArrayElementAtIndex(i);

                    SerializedProperty multiplier = element.FindPropertyRelative("Multiplier");
                    SerializedProperty offset = element.FindPropertyRelative("Offset");
                    SerializedProperty relateInversely = element.FindPropertyRelative("RelateInversely");

                    GUIContent contentAtIndex = GetContentForControlTargetAtIndex(i);

                    float previousLabelWidth = EditorGUIUtility.labelWidth;
                    float previousFieldWidth = EditorGUIUtility.fieldWidth;
                    EditorGUIUtility.labelWidth = 0f;
                    EditorGUIUtility.fieldWidth = 10f;

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(contentAtIndex, GUILayout.Width(previousLabelWidth - 5f));

                    EditorGUILayout.LabelField(scaleContent, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(EditorStyles.label.CalcSize(scaleContent).x));
                    EditorGUILayout.PropertyField(multiplier, GUIContent.none, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(fieldMinSize));

                    EditorGUILayout.LabelField(offsetContent, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(EditorStyles.label.CalcSize(offsetContent).x));
                    EditorGUILayout.PropertyField(offset, GUIContent.none, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(fieldMinSize));

                    EditorGUILayout.LabelField(relateInverslyContent, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .275f), GUILayout.MinWidth(EditorStyles.label.CalcSize(relateInverslyContent).x));
                    EditorGUILayout.PropertyField(relateInversely, GUIContent.none, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .1f), GUILayout.MinWidth(10f));

                    EditorGUILayout.EndHorizontal();
                        
                    EditorGUIUtility.labelWidth = previousLabelWidth;
                    EditorGUIUtility.fieldWidth = previousFieldWidth;
                }
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(rateOfChangeContent, EditorUtils.RichTextStyle);
            
            EditorGUI.BeginChangeCheck();
            ValueGenerationMode roCGenerationMode = (ValueGenerationMode)EditorGUILayout.EnumPopup(roCGenerationModeLabelContent, EditorUtils.GetValueGenerationMode(currentProperty.CurvedRoCProperty, currentProperty.ListedRoCProperty));

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtils.SetValueGenerationMode(currentProperty.CurvedRoCProperty, currentProperty.ListedRoCProperty, roCGenerationMode);
                serializedObject.ApplyModifiedProperties();
            }
            
            Rect rocModeRect = EditorGUILayout.GetControlRect();

            GUIContent label = EditorGUI.BeginProperty(rocModeRect, roCMode, currentProperty.RocModeProperty);
            currentProperty.RocModeProperty.intValue = (int)(RoCMode)EditorGUI.EnumPopup(rocModeRect, label, (RoCMode)currentProperty.RocModeProperty.intValue);
            EditorGUI.EndProperty();
            
            if (currentProperty.CurvedRoCUseControlProperty.boolValue)
            {
                EditorGUILayout.PropertyField(currentProperty.CurvedRoCProperty);
            }
            else if (currentProperty.ListedRoCUseControlProperty.boolValue)
            {
                EditorGUILayout.PropertyField(currentProperty.ListedRoCProperty);
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(miscellaneousContent, EditorUtils.RichTextStyle);

            currentProperty.NoSmoothTransitionsProperty.boolValue = EditorGUILayout.Toggle("Don't Transition Smooth", currentProperty.NoSmoothTransitionsProperty.boolValue);
            currentProperty.UseLightIntensityFloorProperty.boolValue = EditorGUILayout.Toggle("Use Intensity Floor", currentProperty.UseLightIntensityFloorProperty.boolValue);

            if (currentProperty.UseLightIntensityFloorProperty.boolValue)
            {
                currentProperty.FloorProperty.floatValue = EditorGUILayout.FloatField("Intensity Floor", currentProperty.FloorProperty.floatValue);
            }

            currentProperty.UseLightIntensityCeilingProperty.boolValue = EditorGUILayout.Toggle("Use Intensity Ceiling", currentProperty.UseLightIntensityCeilingProperty.boolValue);

            if (currentProperty.UseLightIntensityCeilingProperty.boolValue)
            {
                currentProperty.CeilingProperty.floatValue = EditorGUILayout.FloatField("Intensity Ceiling", currentProperty.CeilingProperty.floatValue);
            }

            if (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth < 350f)
            {
                EditorGUILayout.EndScrollView();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Initialize(SerializedObject property)
        {
            currentProperty = new PropertyData
            {
                ControlTargetProperty = serializedObject.FindProperty("controlTarget"),
                CurvedIntensitiesProperty = serializedObject.FindProperty("curvedIntensities"),
                ListedIntensitiesProperty = serializedObject.FindProperty("listedIntensities"),
                CurvedIntensitiesUseControlProperty = serializedObject.FindProperty("curvedIntensities.useControl"),
                ListedIntensitiesUseControlProperty = serializedObject.FindProperty("listedIntensities.useControl"),
                RocModeProperty = serializedObject.FindProperty("roCMode"),
                CurvedRoCProperty = serializedObject.FindProperty("curvedRoC"),
                ListedRoCProperty = serializedObject.FindProperty("listedRoC"),
                CurvedRoCUseControlProperty = serializedObject.FindProperty("curvedRoC.useControl"),
                ListedRoCUseControlProperty = serializedObject.FindProperty("listedRoC.useControl"),
                IntensityModifersProperty = serializedObject.FindProperty("intensityModifers"),
                NoSmoothTransitionsProperty = serializedObject.FindProperty("noSmoothTransitions"),
                UseLightIntensityFloorProperty = serializedObject.FindProperty("useLightIntensityFloor"),
                UseLightIntensityCeilingProperty = serializedObject.FindProperty("useLightIntensityCeiling"),
                FloorProperty = serializedObject.FindProperty("floor"),
                CeilingProperty = serializedObject.FindProperty("ceiling")
            };
        }
        
        private int GenerationModeConversion(ValueGenerationMode generationMode)
        {
            switch(generationMode)
            {
                case ValueGenerationMode.Constant:
                    return 0;
                case ValueGenerationMode.Curve:
                    return 1;
                case ValueGenerationMode.BetweenCurves:
                    return 2;
                case ValueGenerationMode.BetweenValues:
                    return 3;
                default:
                    throw new System.Exception();
            }
        }

        private IntensityControlTarget GetControlTargetFromIndex(int index)
        {
            return (IntensityControlTarget)Mathf.Pow(2f, index);
        }

        private GUIContent GetContentForControlTargetAtIndex(int index)
        {
            IntensityControlTarget target = GetControlTargetFromIndex(index);

            switch(target)
            {
                case IntensityControlTarget.LightIntensity:
                    return lightIntensityControlTargetContent;
                case IntensityControlTarget.LightColorIntensity:
                    return lightColorIntensityControlTargetContent;
                case IntensityControlTarget.MaterialColorIntensity:
                    return materialColorIntensityControlTargetContent;
                case IntensityControlTarget.LightRange:
                    return lightRangeControlTargetContent;
                case IntensityControlTarget.SpotlightAngle:
                    return spotlightAngleControlTargetContent;
                default:
                    throw new Exception("Added control targets but didn't add them here");
            }
        }
    }
}

#endif