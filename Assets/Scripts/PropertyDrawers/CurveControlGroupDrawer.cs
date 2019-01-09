#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;

using LightControls.ControlOptions.ControlGroups.Data;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(CurveControlGroupData))]
    public class CurveControlGroupDrawer : PropertyDrawer
    {
        private static readonly GUIContent intensityListElementContent;
        private static readonly GUIContent singleCurveContent;
        private static readonly GUIContent doubleCurveContent;
        private static readonly GUIContent timeScaleContent;

        static CurveControlGroupDrawer()
        {
            intensityListElementContent = new GUIContent("Rate of Change");
            singleCurveContent = new GUIContent("Single Curve", "Remember--be sure to make the graph continuious (not clamped) you'll want the graph to loop or ping-pong. You can change this by opening the curve editor and clicking on the gear near the end of the curve.");
            doubleCurveContent = new GUIContent("Double Curve", "Remember--be sure to make the graph continuious (not clamped) you'll want the graph to loop or ping-pong. You can change this by opening the curve editor and clicking on the gear near the end of the curve.");
            timeScaleContent = new GUIContent("Time Scale");
        }

        private class PropertyData
        {
            public float PreviousMultiplier; //Needs to be tracked in case it changes

            public SerializedProperty CurveControlGroup;
            public SerializedProperty MinMaxCurve;
            public SerializedProperty MinCurve;
            public SerializedProperty MaxCurve;
            public SerializedProperty MinConstant;
            public SerializedProperty MaxConstant;
            public SerializedProperty Mode;
            public SerializedProperty CurveMultipler;
            public SerializedProperty MaxValue;
            public SerializedProperty MinValue;
            public SerializedProperty MaxCurveTrackerPoints;
            public SerializedProperty MinCurveTrackerPoints;
            public SerializedProperty MaxCurveTrackerWrapMode;
            public SerializedProperty MinCurveTrackerWrapMode;
        }
        
        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            ParticleSystemCurveMode curveMode = (ParticleSystemCurveMode)currentProperty.Mode.intValue;
            float totalCurveTime = 0f;

            int indent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;
            
            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorUtils.LineHeight);

            GUIContent curveContent = curveMode == ParticleSystemCurveMode.Constant || curveMode == ParticleSystemCurveMode.TwoConstants
                ? intensityListElementContent
                : curveMode == ParticleSystemCurveMode.Curve
                    ? singleCurveContent
                    : doubleCurveContent;

            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(indentedPosition, currentProperty.MinMaxCurve, curveContent);
            
            bool curveChanged = EditorGUI.EndChangeCheck() || currentProperty.CurveMultipler.floatValue != currentProperty.PreviousMultiplier;

            if (IsCurveBased())
            {
                RedrawAnimationCurveFieldPreview(indentedPosition); //if the user didn't use the built in MinMaxCurve mode switcher, this will force the GUI to update

                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.numberField, 1);

                EditorGUI.BeginChangeCheck();
                totalCurveTime = EditorGUI.FloatField(indentedPosition, timeScaleContent, GetTotalCycleTime().Value);
                bool timeChanged = EditorGUI.EndChangeCheck();

                if(curveChanged || timeChanged)
                {
                    if (timeChanged && totalCurveTime > 0f)
                    {
                        RescaleTime(totalCurveTime);
                    }

                    InitializeCurve();
                }
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            if (IsCurveBased())
            {
                return (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2;//(EditorUtils.DefaultStyle.lineHeight * 2 + EditorUtils.VerticalBuffer);
            }
            else
            {
                return (EditorUtils.LineHeight + EditorUtils.VerticalBuffer); //(EditorUtils.DefaultStyle.lineHeight);
            }
        }

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    PreviousMultiplier = property.FindPropertyRelative("minMaxCurve.m_CurveMultiplier").floatValue,
                    CurveControlGroup = property,
                    MinMaxCurve = property.FindPropertyRelative("minMaxCurve"),
                    MinCurve = property.FindPropertyRelative("minMaxCurve.m_CurveMin"),
                    MaxCurve = property.FindPropertyRelative("minMaxCurve.m_CurveMax"),
                    MinConstant = property.FindPropertyRelative("minMaxCurve.m_ConstantMin"),
                    MaxConstant = property.FindPropertyRelative("minMaxCurve.m_ConstantMax"),
                    Mode = property.FindPropertyRelative("minMaxCurve.m_Mode"),
                    CurveMultipler = property.FindPropertyRelative("minMaxCurve.m_CurveMultiplier"),
                    MaxValue = property.FindPropertyRelative("maxValue"),
                    MinValue = property.FindPropertyRelative("minValue"),
                    MaxCurveTrackerPoints = property.FindPropertyRelative("maxCurveTracker.minMaxPoints"),
                    MinCurveTrackerPoints = property.FindPropertyRelative("minCurveTracker.minMaxPoints"),
                    MaxCurveTrackerWrapMode = property.FindPropertyRelative("maxCurveTracker.wrapMode"),
                    MinCurveTrackerWrapMode = property.FindPropertyRelative("minCurveTracker.wrapMode")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);

                InitializeCurve();
            }
        }

        private void InitializeCurve()
        {
            currentProperty.MaxCurveTrackerPoints.SetArray(GetMaxCurveMinMaxPoints());
            currentProperty.MinCurveTrackerPoints.SetArray(GetMinCurveMinMaxPoints());
            CalculateAbsoluteMinMaxValues();
            UpdateWrapMode();
        }

        private float[] GetMaxCurveMinMaxPoints()
        {
            float[] minMaxPoints = Utilities.MathUtils.GetMinMaxTimes(currentProperty.MaxCurve.animationCurveValue); //this is the same curve as MinMaxCurve.curve

            return minMaxPoints;
        }

        private float[] GetMinCurveMinMaxPoints()
        {
            float[] minMaxPoints = Utilities.MathUtils.GetMinMaxTimes(currentProperty.MinCurve.animationCurveValue);

            return minMaxPoints;
        }

        private void CalculateAbsoluteMinMaxValues()
        {
            ParticleSystemCurveMode currentMode = (ParticleSystemCurveMode)currentProperty.Mode.intValue;

            if (currentMode == ParticleSystemCurveMode.Curve)
            {
                var minMaxValues = currentProperty.MaxCurveTrackerPoints.GetArray<float>()
                    .Select(point => currentProperty.MaxCurve.animationCurveValue.Evaluate(point))
                    .Concat(new float[] { currentProperty.MaxCurve.animationCurveValue[0].value, currentProperty.MaxCurve.animationCurveValue[currentProperty.MaxCurve.animationCurveValue.length - 1].value });

                currentProperty.MaxValue.floatValue = minMaxValues.Max() * currentProperty.CurveMultipler.floatValue;
                currentProperty.MinValue.floatValue = minMaxValues.Min() * currentProperty.CurveMultipler.floatValue;
            }
            else if (currentMode == ParticleSystemCurveMode.TwoCurves)
            {
                var minMaxValues = currentProperty.MaxCurveTrackerPoints.GetArray<float>()
                    .Select(point => currentProperty.MaxCurve.animationCurveValue.Evaluate(point))
                    .Concat(new float[] { currentProperty.MaxCurve.animationCurveValue[0].value, currentProperty.MaxCurve.animationCurveValue[currentProperty.MaxCurve.animationCurveValue.length - 1].value })
                    .Concat(currentProperty.MinCurveTrackerPoints.GetArray<float>()
                        .Select(point => currentProperty.MinCurve.animationCurveValue.Evaluate(point)))
                    .Concat(new float[] { currentProperty.MinCurve.animationCurveValue[0].value, currentProperty.MinCurve.animationCurveValue[currentProperty.MinCurve.animationCurveValue.length - 1].value });

                currentProperty.MaxValue.floatValue = minMaxValues.Max() * currentProperty.CurveMultipler.floatValue;
                currentProperty.MinValue.floatValue = minMaxValues.Min() * currentProperty.CurveMultipler.floatValue;
            }
            else if (currentMode == ParticleSystemCurveMode.TwoConstants)
            {
                currentProperty.MaxValue.floatValue = currentProperty.MaxConstant.floatValue;
                currentProperty.MinValue.floatValue = currentProperty.MinConstant.floatValue;
            }
            else
            {
                currentProperty.MaxValue.floatValue = currentProperty.MaxConstant.floatValue;
                currentProperty.MinValue.floatValue = currentProperty.MaxConstant.floatValue;
            }
        }

        private void UpdateWrapMode()
        {
            currentProperty.MaxCurveTrackerWrapMode.intValue = (int)currentProperty.MaxCurve.animationCurveValue.postWrapMode.ConvertWrapMode();

            currentProperty.MinCurveTrackerWrapMode.intValue = (int)currentProperty.MinCurve.animationCurveValue.postWrapMode.ConvertWrapMode();
        }

        private float? GetTotalCycleTime()
        {
            if ((ParticleSystemCurveMode)currentProperty.Mode.intValue == ParticleSystemCurveMode.Curve)
            {
                var totalTime = currentProperty.MaxCurve.animationCurveValue[currentProperty.MaxCurve.animationCurveValue.length - 1].time - currentProperty.MaxCurve.animationCurveValue[0].time;

                return totalTime;
            }
            else if ((ParticleSystemCurveMode)currentProperty.Mode.intValue == ParticleSystemCurveMode.TwoCurves)
            {
                var totalTime1 = currentProperty.MaxCurve.animationCurveValue[currentProperty.MaxCurve.animationCurveValue.length - 1].time - currentProperty.MaxCurve.animationCurveValue[0].time;
                var totalTime2 = currentProperty.MinCurve.animationCurveValue[currentProperty.MinCurve.animationCurveValue.length - 1].time - currentProperty.MinCurve.animationCurveValue[0].time;

                return Mathf.Max(totalTime1, totalTime2);
            }
            else
            {
                return null;
            }
        }

        private void RescaleTime(float totalCycleTime)
        {
            if ((ParticleSystemCurveMode)currentProperty.Mode.intValue == ParticleSystemCurveMode.Curve)
            {
                var animationCurve = currentProperty.MaxCurve.animationCurveValue;
                animationCurve.RescaleTime(totalCycleTime);
                currentProperty.MaxCurve.animationCurveValue = animationCurve;
            }
            else if ((ParticleSystemCurveMode)currentProperty.Mode.intValue == ParticleSystemCurveMode.TwoCurves)
            {
                var animationCurve = currentProperty.MinCurve.animationCurveValue;
                animationCurve.RescaleTime(totalCycleTime);
                currentProperty.MinCurve.animationCurveValue = animationCurve;

                animationCurve = currentProperty.MaxCurve.animationCurveValue;
                animationCurve.RescaleTime(totalCycleTime);
                currentProperty.MaxCurve.animationCurveValue = animationCurve;
            }
        }

        private void RedrawAnimationCurveFieldPreview(Rect position)
        {
            int id = currentProperty.MaxCurve.GetHashCode();

            if (Event.current.GetTypeForControl(id) == EventType.Repaint)
            {
                ParticleSystemCurveMode curveMode = (ParticleSystemCurveMode)currentProperty.Mode.intValue;

                GUIStyle colorPickerBox = GUI.skin.FindStyle("ColorPickerBox");
                colorPickerBox = colorPickerBox ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ColorPickerBox");

                position.xMin += EditorGUIUtility.labelWidth;
                position.xMax -= 18f;

                if (curveMode == ParticleSystemCurveMode.Curve)
                {
                    EditorGUIUtility.DrawCurveSwatch(position, currentProperty.MaxCurve.animationCurveValue, null, Color.green, new Color(0.337f, 0.337f, 0.337f, 1f));
                }
                else
                {
                    EditorGUIUtility.DrawRegionSwatch(position, currentProperty.MaxCurve.animationCurveValue, currentProperty.MinCurve.animationCurveValue, Color.green, new Color(0.337f, 0.337f, 0.337f, 1f), GetCurveRanges());
                }

                colorPickerBox.Draw(position, GUIContent.none, id, true);
            }
        }

        private Rect GetCurveRanges()
        {
            float minTime = currentProperty.MaxCurve.animationCurveValue[0].time;
            minTime = Mathf.Min(minTime, currentProperty.MinCurve.animationCurveValue[0].time);

            float maxTime = currentProperty.MaxCurve.animationCurveValue[currentProperty.MaxCurve.animationCurveValue.length - 1].time;
            maxTime = Mathf.Max(maxTime, currentProperty.MinCurve.animationCurveValue[currentProperty.MinCurve.animationCurveValue.length - 1].time);

            var significantValues = ConvertMinMaxTimes(currentProperty.MaxCurve.animationCurveValue, currentProperty.MaxCurveTrackerPoints.GetArray<float>())
                .Concat(ConvertMinMaxTimes(currentProperty.MinCurve.animationCurveValue, currentProperty.MinCurveTrackerPoints.GetArray<float>()));
            
            float minValue = significantValues.Min();
            float maxValue = significantValues.Max();

            return Rect.MinMaxRect(minTime, minValue, maxTime, maxValue);
        }

        private float[] ConvertMinMaxTimes(AnimationCurve curve, float[] times)
        {
            for (int i = 0; i < times.Length; i++)
            {
                if (i == 0)
                {
                    times[i] = curve[0].value;
                }
                else if (i == times.Length - 1)
                {
                    times[i] = curve[curve.length - 1].value;
                }
                else
                {
                    times[i] = curve.Evaluate(times[i]);
                }
            }

            return times;
        }
        
        private bool IsCurveBased()
        {
            ParticleSystemCurveMode curveMode = (ParticleSystemCurveMode)currentProperty.Mode.intValue;

            return curveMode == ParticleSystemCurveMode.Curve || curveMode == ParticleSystemCurveMode.TwoCurves;
        }
    }
}

#endif