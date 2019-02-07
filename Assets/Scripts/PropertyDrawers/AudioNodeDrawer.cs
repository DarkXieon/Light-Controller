#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AudioNode))]
    public class AudioNodeDrawer : PropertyDrawer
    {
        private class PropertyData
        {
            public string ClipAssetId;

            public SerializedProperty Clip;
            public SerializedProperty PlayAtIntensity;
            public SerializedProperty AudioStart;
            public SerializedProperty AudioEnd;
            public SerializedProperty MinVolume;
            public SerializedProperty MaxVolume;
            public SerializedProperty MinSampleVolume;
            public SerializedProperty MaxSampleVolume;
            public SerializedProperty Loop;
            public SerializedProperty ChangeVolumeFromIntensity;
        }
        
        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        private readonly GUIContent arrayLengthContent = new GUIContent("Number of Nodes: ", "");
        private readonly GUIContent clipContent = new GUIContent("Clip: ", "");
        private readonly GUIContent playAtIntensityContent = new GUIContent("Play at: ", "");
        private readonly GUIContent loopContent = new GUIContent("Loop: ", "");
        private readonly GUIContent changeVolumeFromIntensityContent = new GUIContent("Intensity Volume: ", "");
        private readonly GUIContent audioStartConent = new GUIContent("Start at: ", "");
        private readonly GUIContent audioEndConent = new GUIContent("End at: ", "");
        private readonly GUIContent maxVolumeConent = new GUIContent("Max Volume: ", "");
        private readonly GUIContent minVolumeConent = new GUIContent("Min Volume: ", "");
        private readonly GUIContent volumeConent = new GUIContent("Volume: ", "");

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

            Rect[] lineOneRects = EditorUtils.SplitLineRect(
                lineRect: indentedPosition,
                labelContent: new GUIContent[] { clipContent },
                labelStyle: EditorStyles.label,
                linesDownward: 0);

            EditorGUI.LabelField(lineOneRects[0], clipContent);

            EditorGUI.BeginChangeCheck();
            EditorGUI.ObjectField(lineOneRects[1], currentProperty.Clip, typeof(AudioClip), GUIContent.none);
            currentProperty.Clip.serializedObject.ApplyModifiedProperties();

            if(EditorGUI.EndChangeCheck())
            {
                AudioClip clip = (AudioClip)currentProperty.Clip.objectReferenceValue;
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);
                currentProperty.MinSampleVolume.floatValue = samples.Select(sample => Mathf.Abs(sample)).Min();
                currentProperty.MaxSampleVolume.floatValue = samples.Max();
            }

            if (currentProperty.Clip.objectReferenceValue != null)
            {
                //Rect[] lineTwoRects = EditorUtils.SplitLineRect(
                //    lineRect: indentedPosition,
                //    labelContent: new GUIContent[] { playAtIntensityContent, loopContent, changeVolumeFromIntensityContent },
                //    sizePercentages: new float[] { .50f, .15f, .35f },
                //    labelStyle: EditorStyles.label,
                //    linesDownward: 1);

                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.label);

                Rect[] lineTwoRects = EditorUtils.ScaledSplitLineRect(indentedPosition, new GUIContent[] { playAtIntensityContent, loopContent, changeVolumeFromIntensityContent }, new EditorUtils.FieldType[] { EditorUtils.FieldType.Text, EditorUtils.FieldType.Toggle, EditorUtils.FieldType.Toggle }, EditorStyles.label);

                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.label);

                Rect[] lineThreeRects = EditorUtils.SplitLineRect(
                    lineRect: indentedPosition,
                    labelContent: new GUIContent[] { audioStartConent, audioEndConent },
                    labelStyle: EditorStyles.label,
                    linesDownward: 0);

                EditorGUI.LabelField(lineTwoRects[0], playAtIntensityContent);
                EditorGUI.PropertyField(lineTwoRects[1], currentProperty.PlayAtIntensity, GUIContent.none);

                EditorGUI.LabelField(lineTwoRects[2], loopContent);
                EditorGUI.PropertyField(lineTwoRects[3], currentProperty.Loop, GUIContent.none);

                EditorGUI.LabelField(lineTwoRects[4], changeVolumeFromIntensityContent);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(lineTwoRects[5], currentProperty.ChangeVolumeFromIntensity, GUIContent.none);

                if (EditorGUI.EndChangeCheck() && currentProperty.ChangeVolumeFromIntensity.boolValue)
                {
                    if(!currentProperty.MinVolume.hasMultipleDifferentValues)
                        currentProperty.MinVolume.floatValue = currentProperty.MaxVolume.floatValue > 0f
                            ? currentProperty.MaxVolume.floatValue - 0.01f
                            : 0f;

                    if (!currentProperty.MaxVolume.hasMultipleDifferentValues)
                        currentProperty.MaxVolume.floatValue = currentProperty.MaxVolume.floatValue > 0f
                            ? currentProperty.MaxVolume.floatValue
                            : 0.01f;
                }

                EditorGUI.LabelField(lineThreeRects[0], audioStartConent);
                EditorGUI.Slider(lineThreeRects[1], currentProperty.AudioStart, 0f, (currentProperty.Clip.objectReferenceValue as AudioClip).length - 0.01f, GUIContent.none);

                if (currentProperty.AudioStart.floatValue > currentProperty.AudioEnd.floatValue)
                {
                    currentProperty.AudioEnd.floatValue = currentProperty.AudioStart.floatValue + 0.01f;
                }

                EditorGUI.LabelField(lineThreeRects[2], audioEndConent);
                EditorGUI.Slider(lineThreeRects[3], currentProperty.AudioEnd, 0.01f, (currentProperty.Clip.objectReferenceValue as AudioClip).length, GUIContent.none);

                if (currentProperty.AudioEnd.floatValue < currentProperty.AudioStart.floatValue)
                {
                    currentProperty.AudioStart.floatValue = currentProperty.AudioEnd.floatValue - 0.01f;
                }

                indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.label);

                Rect[] lineFourRects = currentProperty.ChangeVolumeFromIntensity.boolValue
                    ? EditorUtils.SplitLineRect(
                        lineRect: indentedPosition,
                        labelContent: new GUIContent[] { minVolumeConent, maxVolumeConent },
                        labelStyle: EditorStyles.label,
                        linesDownward: 0)
                    : EditorUtils.SplitLineRect(
                        lineRect: indentedPosition,
                        labelContent: new GUIContent[] { volumeConent },
                        labelStyle: EditorStyles.label,
                        linesDownward: 0);

                if (currentProperty.ChangeVolumeFromIntensity.boolValue)
                {
                    EditorGUI.LabelField(lineFourRects[0], minVolumeConent);
                    EditorGUI.Slider(lineFourRects[1], currentProperty.MinVolume, 0f, 0.99f, GUIContent.none);

                    if (currentProperty.MinVolume.floatValue >= currentProperty.MaxVolume.floatValue)
                    {
                        currentProperty.MaxVolume.floatValue = currentProperty.MinVolume.floatValue + 0.01f;
                    }

                    EditorGUI.LabelField(lineFourRects[2], maxVolumeConent);
                    EditorGUI.Slider(lineFourRects[3], currentProperty.MaxVolume, 0.01f, 1f, GUIContent.none);

                    if (currentProperty.MaxVolume.floatValue <= currentProperty.MinVolume.floatValue)
                    {
                        currentProperty.MinVolume.floatValue = currentProperty.MaxVolume.floatValue - 0.01f;
                    }
                }
                else
                {
                    EditorGUI.LabelField(lineFourRects[0], volumeConent);
                    EditorGUI.Slider(lineFourRects[1], currentProperty.MaxVolume, 0f, 1f, GUIContent.none);
                }
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUIUtility.fieldWidth = previousFieldWidth;

            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty currentProperty, GUIContent label)
        {
            return (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * EditorGUIUtility.pixelsPerPoint * 4;
        }

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    ClipAssetId = property.FindPropertyRelative("Clip").objectReferenceValue != null
                        ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(property.FindPropertyRelative("Clip").objectReferenceValue))
                        : null,
                    Clip = property.FindPropertyRelative("Clip"),
                    PlayAtIntensity = property.FindPropertyRelative("PlayAtIntensity"),
                    AudioStart = property.FindPropertyRelative("AudioStart"),
                    AudioEnd = property.FindPropertyRelative("AudioEnd"),
                    MinVolume = property.FindPropertyRelative("MinVolume"),
                    MaxVolume = property.FindPropertyRelative("MaxVolume"),
                    MinSampleVolume = property.FindPropertyRelative("MinSampleVolume"),
                    MaxSampleVolume = property.FindPropertyRelative("MaxSampleVolume"),
                    Loop = property.FindPropertyRelative("Loop"),
                    ChangeVolumeFromIntensity = property.FindPropertyRelative("ChangeVolumeFromIntensity")
                };
                
                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }
    }
}

#endif