#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;
using static LightControls.Utilities.EditorUtils;

namespace LightControls.Editors
{
    [InitializeOnLoad]
    [CustomEditor(typeof(StagedControlOption))]
    public class StagedControlOptionEditor : Editor
    {
        private static readonly GUIContent headingContent;
        private static readonly GUIContent randomizeStageOrderContent;
        private static readonly GUIContent allowLooseOrderContent;
        private static readonly GUIContent stagesLengthContent;
        private static readonly GUIContent stagesElementContent;

        static StagedControlOptionEditor()
        {
            headingContent = new GUIContent(EditorUtils.GetRichText("Staged Control Option", 12, TextOptions.Bold), "");
            randomizeStageOrderContent = new GUIContent("Randomize Stage Order", "");
            allowLooseOrderContent = new GUIContent("Allow Loose Order", "");
            stagesLengthContent = new GUIContent("Stages", "");
            stagesElementContent = new GUIContent("Stage", "");
        }

        private class PropertyData
        {
            public SerializedProperty StagesProperty;
            public SerializedProperty RandomizeNextStageProperty;
            public SerializedProperty CanSkipStagesProperty;
        }

        private PropertyData currentProperty;

        public override void OnInspectorGUI()
        {
            //Initialize(serializedObject);

            //StagedControlOption mainTarget = ((StagedControlOption)target);

            StagedControlOption[] allTargets = targets
                .Cast<StagedControlOption>()
                .ToArray();

            ////allTargets.ToList().ForEach(targ => targ.OnEnable());

            //EditorGUILayout.LabelField(headingContent, EditorUtils.RichTextStyle);
            //EditorGUILayout.Space();

            //currentProperty.RandomizeNextStageProperty.boolValue = EditorGUILayout.Toggle(randomizeStageOrderContent, currentProperty.RandomizeNextStageProperty.boolValue);

            //currentProperty.CanSkipStagesProperty.boolValue = EditorGUILayout.Toggle(allowLooseOrderContent, currentProperty.CanSkipStagesProperty.boolValue);

            //int size = EditorUtils.ArraySizeField(stagesLengthContent, mainTarget.Stager.Stages.Length, allTargets.Length > 1);
            //EditorGUILayout.Space();

            ////Array.Resize(ref mainTarget.Stages, size);

            //mainTarget.Stager.Stages = MiscUtils.ResizeAndFill(mainTarget.Stager.Stages, size);

            ////mainTarget.Stages = mainTarget.Stages.Select(stage => stage == null ? new ControlOptionStage() : stage).ToArray();

            //serializedObject.Update();

            //EditorUtils.DisplayMultilinePropertyArray(currentProperty.StagesProperty, stagesElementContent, true, true);

            //serializedObject.ApplyModifiedProperties();

            //for (int i = 0; i < mainTarget.Stager.Stages.Length; i++)
            //{
            //    PerformChangeToAll(allTargets, currentTarget =>
            //    {
            //        currentTarget.Stager.Stages[i].ControlOptions = currentTarget.Stager.Stages[i].ControlOptions.Select(option => option == null ? Instantiate(GetInstancedOption()) : option).ToArray();
            //    });
            //}

            //serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stager"));

            serializedObject.ApplyModifiedProperties();

            //PerformChangeToAll(allTargets, currentTarget =>
            //{
            //    LightControlOption[] subAssets = currentTarget.Stager.Stages
            //        .SelectMany(stage => stage.ControlOptions)
            //        .ToArray();

            //    SaveSubAssets(currentTarget, subAssets);
            //});

            //EditorUtils.UpdateInstancedLightControls();
        }

        private void Initialize(SerializedObject property)
        {
            currentProperty = new PropertyData
            {
                StagesProperty = serializedObject.FindProperty("stager.stages"),
                RandomizeNextStageProperty = serializedObject.FindProperty("Stager.RandomizeNextStage"),
                CanSkipStagesProperty = serializedObject.FindProperty("Stager.CanSkipStages")
            };
        }

        //public void SaveSubAssets(StagedControlOption mainAsset, LightControlOption[] subAssets)
        //{
        //    string assetPath = AssetDatabase.GetAssetPath(mainAsset);

        //    var oldAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath).ToList();

        //    oldAssets.Remove(mainAsset);
            
        //    bool changed = false;

        //    foreach (LightControlOption oldAsset in oldAssets)
        //    {
        //        if (oldAsset != null && !subAssets.Contains(oldAsset))
        //        {
        //            changed = true;
                    
        //            DestroyImmediate(oldAsset, true);
        //        }
        //    }

        //    foreach (LightControlOption subAsset in subAssets)
        //    {
        //        if(subAsset != null && !oldAssets.Contains(subAsset))
        //        {
        //            changed = true;

        //            subAsset.name = GUID.Generate().ToString();

        //            AssetDatabase.AddObjectToAsset(subAsset, mainAsset);

        //            //subAsset.hideFlags = HideFlags.HideInHierarchy;
        //        }
        //    }

        //    if(changed)
        //    {
        //        AssetDatabase.SaveAssets();
        //    }
        //}

        //private InstancedControlOption GetInstancedOption()
        //{
        //    InstancedControlOption defaultValue = ScriptableObject.CreateInstance<InstancedControlOption>();

        //    ReflectionUtils.SetMemberAtPath(defaultValue, null, "originalOption");
        //    ReflectionUtils.SetMemberAtPath(defaultValue, null, "clonedOption");

        //    return defaultValue;
        //}

        private void PerformChangeToAll(StagedControlOption[] targets, Action<StagedControlOption> change)
        {
            for(int i = 0; i < targets.Length; i++)
            {
                change.Invoke(targets[i]);
            }
        }
    }
}

#endif