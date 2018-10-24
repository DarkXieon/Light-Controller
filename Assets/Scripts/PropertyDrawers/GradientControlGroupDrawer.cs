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
    [CustomPropertyDrawer(typeof(GradientControlGroupData))]
    public class GradientControllerDrawer : PropertyDrawer
    {
        private static readonly GUIContent colorControlContent;
        private static readonly GUIContent gradientWrapModeContent;

        static GradientControllerDrawer()
        {
            colorControlContent = new GUIContent("Color Control", "");
            gradientWrapModeContent = new GUIContent("Wrap Mode");
        }

        private class PropertyData
        {
            public SerializedProperty MinMaxGradient;
            public SerializedProperty MinMaxGradientMode;
            public SerializedProperty MinMaxGradientMin;
            public SerializedProperty MinMaxGradientMax;
            public SerializedProperty MinMaxTracker;
            public SerializedProperty MinMaxTrackerPoints;
            public SerializedProperty MinMaxCurveTrackerMode;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorUtils.GetTopRect(EditorGUI.IndentedRect(position), EditorGUI.GetPropertyHeight(currentProperty.MinMaxGradient));
            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(indentedPosition, currentProperty.MinMaxGradient, colorControlContent);

            if(EditorGUI.EndChangeCheck())
            {
                currentProperty.MinMaxTrackerPoints.SetArray(GetMinMaxPoints(property));
            }

            Rect next = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
            EditorGUI.PropertyField(next, currentProperty.MinMaxCurveTrackerMode, gradientWrapModeContent);

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            float gradientHeight = EditorGUI.GetPropertyHeight(currentProperty.MinMaxGradient);

            return (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * EditorGUIUtility.pixelsPerPoint + gradientHeight;
        }

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    MinMaxGradient = property.FindPropertyRelative("minMaxGradient"),
                    MinMaxGradientMode = property.FindPropertyRelative("minMaxGradient.m_Mode"),
                    MinMaxGradientMin = property.FindPropertyRelative("minMaxGradient.m_GradientMin"),
                    MinMaxGradientMax = property.FindPropertyRelative("minMaxGradient.m_GradientMax"),
                    MinMaxTracker = property.FindPropertyRelative("minMaxTracker"),
                    MinMaxTrackerPoints = property.FindPropertyRelative("minMaxTracker.minMaxPoints"),
                    MinMaxCurveTrackerMode = property.FindPropertyRelative("minMaxTracker.wrapMode")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }

        private float[] GetMinMaxPoints(SerializedProperty property)
        {
            if ((ParticleSystemGradientMode)currentProperty.MinMaxGradientMode.intValue == ParticleSystemGradientMode.Gradient)
            {
                return GetMaxGradientPoints(currentProperty.MinMaxGradientMax.GetGradientValue());
            }
            else
            {
                float[] points = GetMaxGradientPoints(currentProperty.MinMaxGradientMax.GetGradientValue())
                    .Concat(GetMinGradientPoints(currentProperty.MinMaxGradientMin.GetGradientValue()))
                    .Distinct()
                    .OrderBy(time => time)
                    .ToArray();
                
                return points;
            }
        }

        private float[] GetMaxGradientPoints(Gradient maxGradient)
        {
            float[] points = maxGradient.alphaKeys
                .Select(key => key.time)
                .Concat(maxGradient.colorKeys
                    .Select(key => key.time))
                .Distinct()
                .OrderBy(time => time)
                .ToArray();

            return points;
        }

        private float[] GetMinGradientPoints(Gradient minGradient)
        {
            float[] points = minGradient.alphaKeys
                .Select(key => key.time)
                .Concat(minGradient.colorKeys
                    .Select(key => key.time))
                .Distinct()
                .OrderBy(time => time)
                .ToArray();

            return points;
        }
    }
}

#endif