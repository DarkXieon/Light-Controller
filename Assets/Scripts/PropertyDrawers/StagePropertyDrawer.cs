#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightControls.Controllers;
using LightControls.ControlOptions;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(Stage), true)]
    public class StagePropertyDrawer : PropertyDrawer
    {
        private static readonly GUIContent stageActivationContent;
        private static readonly GUIContent stageActivationLimitStarts;
        private static readonly GUIContent stageActivationTotalContent;
        private static readonly GUIContent stageActivationMinContent;
        private static readonly GUIContent stageActivationMaxContent;

        private static readonly GUIContent stageDurationContent;
        private static readonly GUIContent stageDurationTypeContent;
        private static readonly GUIContent stageDurationIterationsContent;
        private static readonly GUIContent stageDurationIterationsMinContent;
        private static readonly GUIContent stageDurationIterationsMaxContent;
        private static readonly GUIContent stageDurationEventContent;

        private static readonly GUIContent generationModeLabelContent;

        private static readonly GUIContent triggerDelayContent;
        private static readonly GUIContent triggerDelayTypeContent;
        private static readonly GUIContent triggerDelayIterationsContent;
        private static readonly GUIContent triggerDelayIterationsMinContent;
        private static readonly GUIContent triggerDelayIterationsMaxContent;
        private static readonly GUIContent triggerDelayEventContent;

        private static readonly GUIContent stageControlsContent;

        static StagePropertyDrawer()
        {
            stageActivationContent = new GUIContent("Stage Activation", "");
            stageActivationLimitStarts = new GUIContent("Limit Starts", "");
            stageActivationTotalContent = new GUIContent("Activation Amount: ", "");
            stageActivationMinContent = new GUIContent("Min: ", "");
            stageActivationMaxContent = new GUIContent("Max: ", "");

            stageDurationContent = new GUIContent("Stage Duration", "");// EditorUtils.GetRichText("Stage Duration", 12, TextOptions.Bold), "");
            stageDurationTypeContent = new GUIContent("Duration Type", "");
            stageDurationIterationsContent = new GUIContent("Number of Iterations: ", "");
            stageDurationIterationsMinContent = new GUIContent("Min: ", "");
            stageDurationIterationsMaxContent = new GUIContent("Max: ", "");
            stageDurationEventContent = new GUIContent("Event Trigger: ", "");

            generationModeLabelContent = new GUIContent("Generation Mode");

            triggerDelayContent = new GUIContent("Stage Advancement", "");// EditorUtils.GetRichText("Stage Advancement", 12, TextOptions.Bold), "");
            triggerDelayTypeContent = new GUIContent("Trigger Type", "");
            triggerDelayIterationsContent = new GUIContent("Number of Iterations: ", "");
            triggerDelayIterationsMinContent = new GUIContent("Min: ", "");
            triggerDelayIterationsMaxContent = new GUIContent("Max: ", "");
            triggerDelayEventContent = new GUIContent("Event Trigger: ", "");

            stageControlsContent = new GUIContent("Stage Controls", "");
        }

        private class PropertyData
        {
            public SerializedProperty ContinueFor;
            public SerializedProperty AdvanceAfter;
            public SerializedProperty ContinueForListedTimeGeneration;
            public SerializedProperty ContinueForCurvedTimeGeneration;
            public SerializedProperty ContinueForIterationsMin;
            public SerializedProperty ContinueForIterationsMax;
            public SerializedProperty ContinueForEventDetectorScript;
            public SerializedProperty ContinueForEventDetectorMethodName;
            public SerializedProperty AdvanceAfterListedTimeGeneration;
            public SerializedProperty AdvanceAfterCurvedTimeGeneration;
            public SerializedProperty AdvanceAfterIterationsMin;
            public SerializedProperty AdvanceAfterIterationsMax;
            public SerializedProperty AdvanceAfterEventDetectorScript;
            public SerializedProperty AdvanceAfterEventDetectorMethodName;
            public SerializedProperty LimitStarts;
            public SerializedProperty MaxStartAmountMin;
            public SerializedProperty MaxStartAmountMax;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.boldLabel);
            EditorGUI.LabelField(indentedPosition, stageActivationContent, EditorStyles.boldLabel);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
            currentProperty.LimitStarts.boolValue = EditorGUI.Toggle(indentedPosition, stageActivationLimitStarts, currentProperty.LimitStarts.boolValue);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);

            EditorGUI.LabelField(indentedPosition, stageActivationTotalContent);

            Rect tempRect = new Rect(indentedPosition.x + EditorGUIUtility.labelWidth, indentedPosition.y, indentedPosition.width - EditorGUIUtility.labelWidth, indentedPosition.height);
            Rect[] splitRects = EditorUtils.SplitLineRect(tempRect, new GUIContent[] { stageActivationMinContent, stageActivationMinContent }, EditorStyles.label, 0);

            EditorGUI.BeginDisabledGroup(!currentProperty.LimitStarts.boolValue);

            EditorGUI.LabelField(splitRects[0], stageActivationMinContent);
            currentProperty.MaxStartAmountMin.intValue = EditorGUI.IntField(splitRects[1], GUIContent.none, currentProperty.MaxStartAmountMin.intValue);

            EditorGUI.LabelField(splitRects[2], stageActivationMaxContent);
            currentProperty.MaxStartAmountMax.intValue = EditorGUI.IntField(splitRects[3], GUIContent.none, currentProperty.MaxStartAmountMax.intValue);

            EditorGUI.EndDisabledGroup();

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.boldLabel);
            indentedPosition.y += EditorUtils.VerticalSpace;
            EditorGUI.LabelField(indentedPosition, stageDurationContent, EditorStyles.boldLabel);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
            StagingOptions chosenOption = (StagingOptions)EditorGUI.EnumPopup(indentedPosition, stageDurationTypeContent, (StagingOptions)currentProperty.ContinueFor.intValue);
            currentProperty.ContinueFor.intValue = (int)chosenOption;

            if (chosenOption == StagingOptions.Iterations)
            {
                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);

                EditorGUI.LabelField(indentedPosition, stageDurationIterationsContent);

                tempRect = new Rect(indentedPosition.x + EditorGUIUtility.labelWidth, indentedPosition.y, indentedPosition.width - EditorGUIUtility.labelWidth, indentedPosition.height);
                splitRects = EditorUtils.SplitLineRect(tempRect, new GUIContent[] { stageDurationIterationsMinContent, stageDurationIterationsMaxContent }, EditorStyles.label, 0);

                EditorGUI.LabelField(splitRects[0], stageDurationIterationsMinContent);
                EditorGUI.PropertyField(splitRects[1], currentProperty.ContinueForIterationsMin, GUIContent.none);

                EditorGUI.LabelField(splitRects[2], stageDurationIterationsMaxContent);
                EditorGUI.PropertyField(splitRects[3], currentProperty.ContinueForIterationsMax, GUIContent.none);
            }
            else if (chosenOption == StagingOptions.Time)
            {
                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);

                EditorGUI.BeginChangeCheck();

                ValueGenerationMode intensityGenerationMode = (ValueGenerationMode)EditorGUI.EnumPopup(indentedPosition, generationModeLabelContent, EditorUtils.GetValueGenerationMode(currentProperty.ContinueForCurvedTimeGeneration, currentProperty.ContinueForListedTimeGeneration));

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtils.SetValueGenerationMode(currentProperty.ContinueForCurvedTimeGeneration, currentProperty.ContinueForListedTimeGeneration, intensityGenerationMode);
                }

                if (currentProperty.ContinueForCurvedTimeGeneration.FindPropertyRelative("useControl").boolValue)
                {
                    indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
                    indentedPosition.height = EditorGUI.GetPropertyHeight(currentProperty.ContinueForCurvedTimeGeneration);

                    EditorGUI.PropertyField(indentedPosition, currentProperty.ContinueForCurvedTimeGeneration);
                }
                else if (currentProperty.ContinueForListedTimeGeneration.FindPropertyRelative("useControl").boolValue)
                {
                    indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
                    indentedPosition.height = EditorGUI.GetPropertyHeight(currentProperty.ContinueForListedTimeGeneration);

                    EditorGUI.PropertyField(indentedPosition, currentProperty.ContinueForListedTimeGeneration);
                }
            }
            else if(chosenOption == StagingOptions.Event)
            {
                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);

                EditorGUI.LabelField(indentedPosition, stageDurationEventContent);

                tempRect = new Rect(indentedPosition.x + EditorGUIUtility.labelWidth, indentedPosition.y, indentedPosition.width - EditorGUIUtility.labelWidth, indentedPosition.height);
                splitRects = EditorUtils.SplitLineRect(tempRect, new GUIContent[] { GUIContent.none, GUIContent.none }, EditorStyles.label, 0);
                
                EditorGUI.PropertyField(splitRects[1], currentProperty.ContinueForEventDetectorScript, GUIContent.none);
                
                MonoBehaviour script = (MonoBehaviour)currentProperty.ContinueForEventDetectorScript.objectReferenceValue;
                    
                MethodInfo[] methods = script != null
                    ? EventDetector.FindMatchingMethods(script).ToArray()
                    : EventDetector.FindMatchingMethods().ToArray();

                string[] methodNames = methods
                    .Select(method => method.Name)
                    .ToArray();

                int currentlySelected = !string.IsNullOrWhiteSpace(currentProperty.ContinueForEventDetectorMethodName.stringValue) && methods.Any(method => method.Name == currentProperty.ContinueForEventDetectorMethodName.stringValue)
                    ? Array.IndexOf(methods, EventDetector.FindMatchingMethod(currentProperty.ContinueForEventDetectorMethodName.stringValue, script))
                    : -1;

                int userSelected = EditorGUI.Popup(splitRects[3], currentlySelected, methodNames);

                if (currentlySelected != userSelected)
                {
                    currentProperty.ContinueForEventDetectorMethodName.stringValue = methodNames[userSelected];
                }
            }

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.boldLabel);
            indentedPosition.y += EditorUtils.VerticalSpace;
            EditorGUI.LabelField(indentedPosition, triggerDelayContent, EditorStyles.boldLabel);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
            chosenOption = (StagingOptions)EditorGUI.EnumPopup(indentedPosition, triggerDelayTypeContent, (StagingOptions)currentProperty.AdvanceAfter.intValue);
            currentProperty.AdvanceAfter.intValue = (int)chosenOption;

            if (chosenOption == StagingOptions.Iterations)
            {
                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);

                EditorGUI.LabelField(indentedPosition, triggerDelayIterationsContent);

                tempRect = new Rect(indentedPosition.x + EditorGUIUtility.labelWidth, indentedPosition.y, indentedPosition.width - EditorGUIUtility.labelWidth, indentedPosition.height);
                splitRects = EditorUtils.SplitLineRect(tempRect, new GUIContent[] { triggerDelayIterationsMinContent, triggerDelayIterationsMaxContent }, EditorStyles.label, 0);

                EditorGUI.LabelField(splitRects[0], triggerDelayIterationsMinContent);
                EditorGUI.PropertyField(splitRects[1], currentProperty.AdvanceAfterIterationsMin, GUIContent.none);

                EditorGUI.LabelField(splitRects[2], triggerDelayIterationsMaxContent);
                EditorGUI.PropertyField(splitRects[3], currentProperty.AdvanceAfterIterationsMax, GUIContent.none);
            }
            else if (chosenOption == StagingOptions.Time)
            {
                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);

                EditorGUI.BeginChangeCheck();

                ValueGenerationMode intensityGenerationMode = (ValueGenerationMode)EditorGUI.EnumPopup(indentedPosition, generationModeLabelContent, EditorUtils.GetValueGenerationMode(currentProperty.AdvanceAfterCurvedTimeGeneration, currentProperty.AdvanceAfterListedTimeGeneration));

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtils.SetValueGenerationMode(currentProperty.AdvanceAfterCurvedTimeGeneration, currentProperty.AdvanceAfterListedTimeGeneration, intensityGenerationMode);
                }

                if (currentProperty.AdvanceAfterCurvedTimeGeneration.FindPropertyRelative("useControl").boolValue)
                {
                    indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
                    indentedPosition.height = EditorGUI.GetPropertyHeight(currentProperty.AdvanceAfterCurvedTimeGeneration);

                    EditorGUI.PropertyField(indentedPosition, currentProperty.AdvanceAfterCurvedTimeGeneration);
                }
                else if (currentProperty.AdvanceAfterListedTimeGeneration.FindPropertyRelative("useControl").boolValue)
                {
                    indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);
                    indentedPosition.height = EditorGUI.GetPropertyHeight(currentProperty.AdvanceAfterListedTimeGeneration);

                    EditorGUI.PropertyField(indentedPosition, currentProperty.AdvanceAfterListedTimeGeneration);
                }
            }
            else if (chosenOption == StagingOptions.Event)
            {
                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.LineHeight);

                EditorGUI.LabelField(indentedPosition, triggerDelayEventContent);

                tempRect = new Rect(indentedPosition.x + EditorGUIUtility.labelWidth, indentedPosition.y, indentedPosition.width - EditorGUIUtility.labelWidth, indentedPosition.height);
                splitRects = EditorUtils.SplitLineRect(tempRect, new GUIContent[] { GUIContent.none, GUIContent.none }, EditorStyles.label, 0);

                EditorGUI.PropertyField(splitRects[1], currentProperty.AdvanceAfterEventDetectorScript, GUIContent.none);
                
                MonoBehaviour script = (MonoBehaviour)currentProperty.AdvanceAfterEventDetectorScript.objectReferenceValue;

                MethodInfo[] methods = EventDetector.FindMatchingMethods(script).ToArray();

                string[] methodNames = methods
                    .Select(method => method.Name)
                    .ToArray();

                int currentlySelected = !string.IsNullOrWhiteSpace(currentProperty.AdvanceAfterEventDetectorMethodName.stringValue) && methods.Any(method => method.Name == currentProperty.AdvanceAfterEventDetectorMethodName.stringValue)
                    ? Array.IndexOf(methods, EventDetector.FindMatchingMethod(currentProperty.AdvanceAfterEventDetectorMethodName.stringValue, script))
                    : -1;

                int userSelected = EditorGUI.Popup(splitRects[3], currentlySelected, methodNames);

                if (currentlySelected != userSelected)
                {
                    currentProperty.AdvanceAfterEventDetectorMethodName.stringValue = methodNames[userSelected];
                }   
            }

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.boldLabel);
            indentedPosition.y += EditorUtils.VerticalSpace;
            EditorGUI.LabelField(indentedPosition, stageControlsContent, EditorStyles.boldLabel);
            
            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            float headingHeight = (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 4f + EditorUtils.VerticalSpace * 3f;

            float activationHeight = (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f;

            ValueGenerationMode currentMode = EditorUtils.GetValueGenerationMode(currentProperty.ContinueForCurvedTimeGeneration, currentProperty.ContinueForListedTimeGeneration);

            float durationHeight = (StagingOptions)currentProperty.ContinueFor.intValue == StagingOptions.None
                ? EditorUtils.LineHeight + EditorUtils.VerticalBuffer
                : (StagingOptions)currentProperty.ContinueFor.intValue == StagingOptions.Time
                    ? currentMode == ValueGenerationMode.None
                        ? (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f
                        : currentMode == ValueGenerationMode.List
                            ? (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f + EditorGUI.GetPropertyHeight(currentProperty.ContinueForListedTimeGeneration)
                            : (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f + EditorGUI.GetPropertyHeight(currentProperty.ContinueForCurvedTimeGeneration)
                    : (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f;

            currentMode = EditorUtils.GetValueGenerationMode(currentProperty.AdvanceAfterCurvedTimeGeneration, currentProperty.AdvanceAfterListedTimeGeneration);

            float triggerHeight = (StagingOptions)currentProperty.AdvanceAfter.intValue == StagingOptions.None
                ? EditorUtils.LineHeight + EditorUtils.VerticalBuffer
                : (StagingOptions)currentProperty.AdvanceAfter.intValue == StagingOptions.Time
                    ? currentMode == ValueGenerationMode.None
                        ? (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f
                        : currentMode == ValueGenerationMode.List
                            ? (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f + EditorGUI.GetPropertyHeight(currentProperty.AdvanceAfterListedTimeGeneration)
                            : (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f + EditorGUI.GetPropertyHeight(currentProperty.AdvanceAfterCurvedTimeGeneration)
                    : (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f;
            
            return headingHeight + activationHeight + durationHeight + triggerHeight + EditorUtils.VerticalBuffer;
        }

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    ContinueFor = property.FindPropertyRelative("ContinueFor"),
                    AdvanceAfter = property.FindPropertyRelative("AdvanceAfter"),
                    ContinueForListedTimeGeneration = property.FindPropertyRelative("ContinueForListedTimeGeneration"),
                    ContinueForCurvedTimeGeneration = property.FindPropertyRelative("ContinueForCurvedTimeGeneration"),
                    ContinueForIterationsMin = property.FindPropertyRelative("ContinueForIterationsMin"),
                    ContinueForIterationsMax = property.FindPropertyRelative("ContinueForIterationsMax"),
                    ContinueForEventDetectorScript = property.FindPropertyRelative("ContinueForEventDetector.detector"),
                    ContinueForEventDetectorMethodName = property.FindPropertyRelative("ContinueForEventDetector.methodName"),
                    AdvanceAfterListedTimeGeneration = property.FindPropertyRelative("AdvanceAfterListedTimeGeneration"),
                    AdvanceAfterCurvedTimeGeneration = property.FindPropertyRelative("AdvanceAfterCurvedTimeGeneration"),
                    AdvanceAfterIterationsMin = property.FindPropertyRelative("AdvanceAfterIterationsMin"),
                    AdvanceAfterIterationsMax = property.FindPropertyRelative("AdvanceAfterIterationsMax"),
                    AdvanceAfterEventDetectorScript = property.FindPropertyRelative("AdvanceAfterEventDetector.detector"),
                    AdvanceAfterEventDetectorMethodName = property.FindPropertyRelative("AdvanceAfterEventDetector.methodName"),
                    LimitStarts = property.FindPropertyRelative("LimitStarts"),
                    MaxStartAmountMin = property.FindPropertyRelative("MaxStartAmountMin"),
                    MaxStartAmountMax = property.FindPropertyRelative("MaxStartAmountMax")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }
    }
}

#endif