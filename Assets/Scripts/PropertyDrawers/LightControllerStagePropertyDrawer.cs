#if UNITY_EDITOR

using System.Collections.Generic;

using LightControls.Controllers;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(LightControllerStage))]
    public class LightControllerStagePropertyDrawer : StagePropertyDrawer
    {
        private static readonly GUIContent stageControlsAmountContent;
        private static readonly GUIContent stageControlsElementContent;

        static LightControllerStagePropertyDrawer()
        {
            stageControlsAmountContent = new GUIContent("Stage Controls", "");
            stageControlsElementContent = new GUIContent("Stage Control", "");
        }

        private class PropertyData
        {
            public SerializedProperty LightControllers;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            base.OnGUI(position, property, label);
            
            int indent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);
            
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            indentedPosition.y += base.GetPropertyHeight(property, label);

            float difference = indentedPosition.x - position.x;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth -= difference;

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();
            
            int size = EditorUtils.ArraySizeField(indentedPosition, stageControlsAmountContent, currentProperty.LightControllers.arraySize, currentProperty.LightControllers.hasMultipleDifferentValues);

            if (EditorGUI.EndChangeCheck())
            {
                LightController[] currentOptions = currentProperty.LightControllers.GetArray<LightController>();
                currentOptions = MiscUtils.ResizeAndFillWith(currentOptions, size, () => null);
                currentProperty.LightControllers.SetArray(currentOptions);
            }

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightControllers, stageControlsElementContent));
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, currentProperty.LightControllers, stageControlsElementContent, typeof(LightController));

            if(EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                UpdateInstancedLightControllerStages(property);
            }

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth += difference;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            float controlsHeight = EditorUtils.LineHeight + EditorUtils.VerticalBuffer + EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightControllers, stageControlsElementContent);

            return base.GetPropertyHeight(property, label) + controlsHeight + EditorUtils.VerticalBuffer;
        }
        
        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    LightControllers = property.FindPropertyRelative("lightControllers")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }

        protected override string GetInstancedPath(SerializedProperty self)
        {
            string[] splitOriginalPath = self.propertyPath.Split('.');
            string instancedStagePath = $"instancedStager.controllerStages.Array.{splitOriginalPath[splitOriginalPath.Length - 1]}";

            return instancedStagePath;
        }

        private string GetSerializedPath(SerializedProperty self)
        {
            string[] splitOriginalPath = self.propertyPath.Split('.');
            string serializedStagePath = $"controllerStager.stages.Array.{splitOriginalPath[splitOriginalPath.Length - 1]}";

            return serializedStagePath;
        }

        private void UpdateInstancedLightControllerStages(SerializedProperty self)
        {
            self.serializedObject.ApplyModifiedProperties();

            string serializedPath = $"{GetSerializedPath(self)}.lightControllers";
            string instancedPath = $"{GetInstancedPath(self)}.clonedLightControllers";
            string iterationsPath = $"{GetInstancedPath(self)}.iterations";

            EditorUtils.UpdateInstancedArray<LightController, LightController>(
                serializedContainers: self.serializedObject.targetObjects,
                instancedContainers: self.serializedObject.targetObjects,
                serializedPath: serializedPath,
                instancedPath: instancedPath,
                iterationsPath: iterationsPath,
                isInstancedVersion: (serialized, instanced) => serialized == instanced,
                getInstancedVersion: serialized =>
                {
                    LightController intanced = serialized.gameObject.scene.name == null
                        ? Object.Instantiate(serialized)
                        : serialized;

                    intanced.gameObject.SetActive(true);
                    intanced.enabled = true;
                    intanced.IsChild = true;

                    return intanced;
                });
            
            self.serializedObject.Update();
        }
    }
}

#endif