#if UNITY_EDITOR

using System.Linq;

using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.Editors
{
    [InitializeOnLoad]
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AudioControlOption))]
    public class AudioControlOptionEditor : Editor
    {
        private static readonly GUIContent mainLabelContent;
        private static readonly GUIContent useIntensityControlsContent;
        private static readonly GUIContent audioGenerationContent;
        private static readonly GUIContent audioSourcePrefabContent;
        private static readonly GUIContent audioNodesContent;
        private static readonly GUIContent audioNodeContent;
        
        static AudioControlOptionEditor()
        {
            mainLabelContent = new GUIContent("Audio Control Options", "");// EditorUtils.GetRichText("Audio Control Options", 12, EditorUtils.TextOptions.Bold), "");
            useIntensityControlsContent = new GUIContent("Use Audio Controls", "");
            audioGenerationContent = new GUIContent("Audio Generation", "");// EditorUtils.GetRichText("Audio Generation", EditorUtils.TextOptions.Bold), "");
            audioSourcePrefabContent = new GUIContent("Audio Source Prefab", "");
            audioNodesContent = new GUIContent("Audio Nodes", "");
            audioNodeContent = new GUIContent("Audio Node", "");
        }
        
        public override void OnInspectorGUI()
        {
            AudioControlOption control = target as AudioControlOption;

            //EditorGUIUtility.labelWidth = 250f;
            
            EditorGUILayout.LabelField(mainLabelContent, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            //control.UseControl = EditorGUILayout.Toggle(useIntensityControlsContent, control.UseControl);
            //EditorGUILayout.Space();

            EditorGUILayout.LabelField(audioGenerationContent, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("IntensityGenerator"), true);

            serializedObject.ApplyModifiedProperties();

            Rect audioSourceRect = EditorGUILayout.GetControlRect();
            EditorGUI.ObjectField(audioSourceRect, serializedObject.FindProperty("AudioSourcePrefab"), typeof(AudioSource), audioSourcePrefabContent);

            SerializedProperty audioNodesLength = serializedObject.FindProperty("AudioNodes.Array.size");

            EditorGUI.BeginChangeCheck();

            int newSize = EditorUtils.ArraySizeField(audioNodesContent, audioNodesLength.intValue, audioNodesLength.hasMultipleDifferentValues);

            if (EditorGUI.EndChangeCheck())
            {
                audioNodesLength.intValue = newSize;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            int length = ArrayLengthToUse(serializedObject);
            EditorUtils.DisplayMultilinePropertyArray(serializedObject.FindProperty("AudioNodes"), audioNodeContent, false, false, length);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            //EditorUtils.UpdateInstancedLightControls();
        }

        private int ArrayLengthToUse(SerializedObject serializedObject)
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                int[] lengths = serializedObject.targetObjects
                    .Select(targetObject => (targetObject as AudioControlOption).AudioNodes.Length)
                    .ToArray();
                
                return lengths.Min();
            }

            return (serializedObject.targetObject as AudioControlOption).AudioNodes.Length;
        }
    }
}

#endif