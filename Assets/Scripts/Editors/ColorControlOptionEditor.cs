#if UNITY_EDITOR

using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;
using static LightControls.Utilities.EditorUtils;

namespace LightControls.Editors
{
    [InitializeOnLoad]
    [CustomEditor(typeof(ColorControlOption))]
    public class ColorControlOptionEditor : Editor
    {
        private static readonly GUIContent mainLabelContent;
        private static readonly GUIContent colorGenerationContent;
        private static readonly GUIContent colorTargetContent;

        private static readonly GUIContent useColorControlsContent;
        private static readonly GUIContent roCOptionsContent;
        private static readonly GUIContent roCGenerationModeLabelContent;
        private static readonly GUIContent roCMode;

        static ColorControlOptionEditor()
        {
            mainLabelContent = new GUIContent(EditorUtils.GetRichText("Color Control Options", 12, EditorUtils.TextOptions.Bold), "");
            colorGenerationContent = new GUIContent(EditorUtils.GetRichText("Color Generation Options", 12, EditorUtils.TextOptions.Bold), "");
            colorTargetContent = new GUIContent("Color Target", "");

            useColorControlsContent = new GUIContent("Use Color Controls", "");
            roCOptionsContent = new GUIContent(EditorUtils.GetRichText("RoC Control Options", 12, EditorUtils.TextOptions.Bold), "");
            roCGenerationModeLabelContent = new GUIContent("Generation Mode");
            roCMode = new GUIContent("RoC Mode");
        }

        public override void OnInspectorGUI()
        {
            ColorControlOption colorControlOptions = (ColorControlOption)target;
            
            EditorGUIUtility.labelWidth = 250f;
            
            EditorGUILayout.LabelField(mainLabelContent, EditorUtils.RichTextStyle);
            EditorGUILayout.Space();

            //colorControlOptions.UseControl = EditorGUILayout.Toggle(useColorControlsContent, colorControlOptions.UseControl);
            //EditorGUILayout.Space();

            EditorGUILayout.LabelField(colorGenerationContent, EditorUtils.RichTextStyle);
            EditorGUILayout.Space();

            EditorGUI.indentLevel++;

            //EditorGUI.BeginChangeCheck();
            EditorUtils.DisplayColorControlTargetField(EditorGUILayout.GetControlRect(), serializedObject.FindProperty("ColorTarget"));
            //colorControlOptions.ColorTarget = (ColorControlTarget)EditorGUILayout.EnumFlagsField(colorTargetContent, colorControlOptions.ColorTarget);
            //bool changed = EditorGUI.EndChangeCheck();

            //if(changed)
            //{
            //    serializedObject.Update();
            //}

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ColorControl"));
            bool changed = EditorGUI.EndChangeCheck();

            //if (changed)
            //{
            //    serializedObject.ApplyModifiedProperties();

            //    colorControlOptions.ColorControl.OnGradientChanged();

            //    serializedObject.Update();
            //}

            EditorGUILayout.Space();

            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField(roCOptionsContent, EditorUtils.RichTextStyle);
            EditorGUILayout.Space();

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            ValueGenerationMode roCGenerationMode = (ValueGenerationMode)EditorGUILayout.EnumPopup(roCGenerationModeLabelContent, EditorUtils.GetValueGenerationMode(serializedObject.FindProperty("CurvedRoC"), serializedObject.FindProperty("ListedRoC")));
            changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                EditorUtils.SetValueGenerationMode(serializedObject.FindProperty("CurvedRoC"), serializedObject.FindProperty("ListedRoC"), roCGenerationMode);
                serializedObject.ApplyModifiedProperties();
            }


            EditorGUI.BeginChangeCheck();
            colorControlOptions.RoCMode = (RoCMode)EditorGUILayout.EnumPopup(roCMode, colorControlOptions.RoCMode);
            changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                serializedObject.Update();
            }

            if (colorControlOptions.CurvedRoC.UseControl)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CurvedRoC"));
            }
            else if (colorControlOptions.ListedRoC.UseControl)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ListedRoC"));
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            
            //EditorUtils.UpdateInstancedLightControls();

            EditorUtility.SetDirty(colorControlOptions);
        }
    }
}

#endif