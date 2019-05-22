#if UNITY_EDITOR

using System;
using System.Linq;
using LightControls.ControlOptions;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

using static LightControls.Utilities.EditorUtils;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(ControlOptionStage))]
    public class ControlOptionStagePropertyDrawer : StagePropertyDrawer
    {
        private static readonly GUIContent stageControlsAmountContent;
        private static readonly GUIContent stageControlsElementContent;

        static ControlOptionStagePropertyDrawer()
        {
            stageControlsAmountContent = new GUIContent("Stage Controls", "");
            stageControlsElementContent = new GUIContent("Stage Control", "");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            SerializedProperty controlOptions = property.FindPropertyRelative("controlOptions");

            int indent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            indentedPosition.y += base.GetPropertyHeight(property, label);

            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            int size = EditorUtils.ArraySizeField(indentedPosition, stageControlsAmountContent, controlOptions.arraySize, controlOptions.hasMultipleDifferentValues);
            
            if (EditorGUI.EndChangeCheck())
            {
                LightControlOption[] currentOptions = controlOptions.GetArray<LightControlOption>();
                currentOptions = MiscUtils.ResizeAndFillWith(currentOptions, size, () => null);
                controlOptions.SetArray(currentOptions);
            }

            EditorGUI.BeginChangeCheck();

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(controlOptions, stageControlsElementContent));
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, controlOptions, stageControlsElementContent);

            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                UpdateInstancedControlOptions(property);
            }

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty controlOptions = property.FindPropertyRelative("controlOptions");

            float controlsHeight = EditorUtils.LineHeight + EditorUtils.VerticalBuffer + EditorUtils.GetSinglelinePropertyArrayHeight(controlOptions, stageControlsElementContent);

            return base.GetPropertyHeight(property, label) + controlsHeight + EditorUtils.VerticalBuffer;
        }

        protected override string GetInstancedPath(SerializedProperty self)
        {
            string[] splitOriginalPath = self.propertyPath.Split('.');
            string instancedStagePath = $"instancedOptionStager.instancedStages.Array.{splitOriginalPath[splitOriginalPath.Length - 1]}";
            
            return instancedStagePath;
        }

        private void UpdateInstancedControlOptions(SerializedProperty self)
        {
            self.serializedObject.ApplyModifiedProperties();

            FoundControl[] foundControls = EditorUtils.FindControls<StagedControlOption>(self);

            LightControlOption[] serializedOptions = new LightControlOption[foundControls.Length];
            InstancedControlOption[] instancedOptions = new InstancedControlOption[foundControls.Length];

            //Debug.Assert(serializedOptions.All(option => serializedOptions.All(otherOption => option == otherOption)), "For some reason not all of your data that SHOULD come from a single option, is not consistient");

            for(int i = 0; i < foundControls.Length; i++)
            {
                serializedOptions[i] = foundControls[i].ControlOption;
                instancedOptions[i] = foundControls[i].InstancedOption;
            }

            string[] splitOriginalPath = self.propertyPath.Split('.');
            string serializedControlsPath = $"{self.propertyPath}.controlOptions";
            string instancedControlsPath = $"instancedOptionStager.instancedStages.Array.{splitOriginalPath[splitOriginalPath.Length - 1]}.controlOptions";
            string iterationsPath = $"instancedOptionStager.instancedStages.Array.{splitOriginalPath[splitOriginalPath.Length - 1]}.iterations";
            
            EditorUtils.UpdateInstancedArray<LightControlOption, InstancedControlOption>(
                serializedContainers: serializedOptions,
                instancedContainers: instancedOptions,
                serializedPath: serializedControlsPath,
                instancedPath: instancedControlsPath,
                iterationsPath: iterationsPath,
                (controlOption, instancedOption) => EditorUtils.InstancedVersionOf(instancedOption, controlOption),
                controlOption => controlOption.GetInstanced());

            self.serializedObject.Update();

            /*
            self.serializedObject.ApplyModifiedProperties();

            FoundControl[] foundControls = EditorUtils.FindControls<StagedControlOption>(self);

            string[] splitOriginalPath = self.propertyPath.Split('.');
            string normalControlsPath = $"{self.propertyPath}.controlOptions";
            string instancedControlsPath = $"instancedOptionStager.instancedStages.Array.{splitOriginalPath[splitOriginalPath.Length - 1]}.controlOptions";

            for (int i = 0; i < foundControls.Length; i++)
            {
                LightControlOption[] controlOptions = ReflectionUtils.GetMemberAtPath<LightControlOption[]>(foundControls[i].ControlOption, normalControlsPath);
                InstancedControlOption[] instancedControlOptions = ReflectionUtils.GetMemberAtPath<InstancedControlOption[]>(foundControls[i].InstancedOption, instancedControlsPath);

                if(controlOptions.Length == instancedControlOptions.Length)
                {
                    for(int k = 0; k < controlOptions.Length; k++)
                    {
                        if(!EditorUtils.InstancedVersionOf(instancedControlOptions[k], controlOptions[k]))
                        {
                            instancedControlOptions[k] = controlOptions[k].GetInstanced();
                        }
                    }
                }
                //else if(controlOptions.Length < instancedControlOptions.Length)
                //{
                //    for (int k = 0; k < instancedControlOptions.Length; k++)
                //    {
                //        if (!EditorUtils.InstancedVersionOf(instancedControlOptions[k], controlOptions[k]))
                //        {
                //            InstancedControlOption[] newOptions = new InstancedControlOption[instancedControlOptions.Length - 1];

                //            for (int j = 0; j < k; j++)
                //            {
                //                newOptions[j] = instancedControlOptions[j];
                //            }
                //            for (int j = k; j < newOptions.Length; j++)
                //            {
                //                newOptions[j] = instancedControlOptions[j + 1];
                //            }

                //            instancedControlOptions = newOptions;
                //            k--;
                //        }
                //    }

                //    if (controlOptions.Length < instancedControlOptions.Length)
                //    {
                //        Array.Resize(ref instancedControlOptions, controlOptions.Length);
                //    }
                //}
                else
                {
                    Array.Resize(ref instancedControlOptions, controlOptions.Length);
                }

                ReflectionUtils.SetMemberAtPath(foundControls[i].Controller, instancedControlOptions, $"{foundControls[i].InstancedPath}.{instancedControlsPath}");
            }

            self.serializedObject.Update();
            */
        }
    }
}

#endif