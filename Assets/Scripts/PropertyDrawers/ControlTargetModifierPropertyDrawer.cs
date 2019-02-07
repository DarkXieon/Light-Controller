#if UNITY_EDITOR

using System.Collections.Generic;
using LightControls.ControlOptions;
using LightControls.Utilities;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(ControlTargetModifier))]
    public class ControlTargetModifierPropertyDrawer : PropertyDrawer
    {
        private static GUIContent intensityModifiersScaleContent;
        private static GUIContent intensityModifiersOffsetContent;
        private static GUIContent intensityModifiersRelateInverselyContent;
        //private static GUIContent[] intensityModifiersElementContent;

        static ControlTargetModifierPropertyDrawer()
        {
            intensityModifiersScaleContent = new GUIContent("Scale: ", "");
            intensityModifiersOffsetContent = new GUIContent("Offset: ", "");
            intensityModifiersRelateInverselyContent = new GUIContent("Relate Inversely: ", "");
            //intensityModifiersElementContent = new GUIContent[]
            //{
            //    new GUIContent("Light Intensity:", ""),
            //    new GUIContent("Light Color Intensity:", ""),
            //    new GUIContent("Light Material Intensity:", ""),
            //    new GUIContent("Light Range:", ""),
            //    new GUIContent("Spotlight Angle:", "")
            //};
        }

        private class PropertyData
        {
            public SerializedProperty MultiplierProperty;
            public SerializedProperty OffsetProperty;
            public SerializedProperty RelateInverselyProperty;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            //int indent = EditorGUI.indentLevel;
            //float previousLabelWidth = EditorGUIUtility.labelWidth;
            //float previousFieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUI.BeginProperty(position, label, property);

            //EditorGUI.LabelField

            //Rect[] splitLine = EditorUtils.SplitLineRect(
            //    lineRect: position,
            //    labelContent: new GUIContent[] { label, intensityModifiersScaleContent, intensityModifiersOffsetContent, intensityModifiersRelateInverselyContent },
            //    sizePercentages: new float[] { .4f, .2f, .2f, .2f },
            //    labelStyle: EditorStyles.label,
            //    linesDownward: 0);
            
            Rect[] splitLine = EditorUtils.ReplicateDefaultSplitLineRect(position, new GUIContent[] { label, intensityModifiersScaleContent, intensityModifiersOffsetContent, intensityModifiersRelateInverselyContent }, new EditorUtils.FieldType[] { EditorUtils.FieldType.Text, EditorUtils.FieldType.Text, EditorUtils.FieldType.Toggle }, EditorStyles.label);

            Debug.Log(position.width > EditorUtils.MinDefaultSpaceNeeded(new GUIContent[] { label, intensityModifiersScaleContent, intensityModifiersOffsetContent, intensityModifiersRelateInverselyContent }, EditorStyles.label));

            EditorGUI.LabelField(splitLine[0], label);

            EditorGUI.LabelField(splitLine[1], intensityModifiersScaleContent);
            currentProperty.MultiplierProperty.floatValue = EditorGUI.FloatField(splitLine[2], currentProperty.MultiplierProperty.floatValue);

            EditorGUI.LabelField(splitLine[3], intensityModifiersOffsetContent);
            currentProperty.OffsetProperty.floatValue = EditorGUI.FloatField(splitLine[4], currentProperty.OffsetProperty.floatValue);

            EditorGUI.LabelField(splitLine[5], intensityModifiersRelateInverselyContent);
            currentProperty.RelateInverselyProperty.boolValue = EditorGUI.Toggle(splitLine[6], currentProperty.RelateInverselyProperty.boolValue);
            
            EditorGUI.EndProperty();
        }
        
        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    Initialize(property);

        //    float afterToggle = currentProperty.HasAuthorityProperty.boolValue
        //        ? EditorUtils.LineHeight * 2 + EditorUtils.GetIntensityModifiersHeight(currentProperty.ControlTargetProperty) + EditorUtils.VerticalBuffer * 3
        //        : 0f;

        //    return EditorUtils.LineHeight + EditorUtils.VerticalBuffer + afterToggle;// * EditorGUIUtility.pixelsPerPoint * 4;
        //}

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    MultiplierProperty = property.FindPropertyRelative("Multiplier"),
                    OffsetProperty = property.FindPropertyRelative("Offset"),
                    RelateInverselyProperty = property.FindPropertyRelative("RelateInversely")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }
    }
}

#endif