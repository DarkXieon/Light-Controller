#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using LightControls.Controllers;
using LightControls.Controllers.Data;
using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(LightControllerGroupData))]
    public class LightControllerGroupDataDrawer : PropertyDrawer
    {
        private static readonly GUIContent controlOptionsLengthContent;
        private static readonly GUIContent controlOptionsElementContent;

        static LightControllerGroupDataDrawer()
        {
            controlOptionsLengthContent = new GUIContent("Control Options", "");
            controlOptionsElementContent = new GUIContent("Control Option", "");
        }

        private class PropertyData
        {
            public SerializedProperty ControlOptionGroupDataProperty;
            public SerializedProperty LightControlOptionsProperty;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            bool optionsChanged = false;

            int indent = EditorGUI.indentLevel;
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            float previousFieldWidth = EditorGUIUtility.fieldWidth;
            
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorUtils.VerticalSpace);
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorGUI.GetPropertyHeight(currentProperty.ControlOptionGroupDataProperty));
            EditorGUI.PropertyField(indentedPosition, currentProperty.ControlOptionGroupDataProperty, true);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.VerticalSpace);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.numberField);
            currentProperty.LightControlOptionsProperty.arraySize = EditorUtils.ArraySizeField(indentedPosition, controlOptionsLengthContent, currentProperty.LightControlOptionsProperty.arraySize);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightControlOptionsProperty, controlOptionsElementContent));
            EditorGUI.BeginChangeCheck();
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, currentProperty.LightControlOptionsProperty, controlOptionsElementContent);
            optionsChanged = EditorGUI.EndChangeCheck();

            if(EditorApplication.isPlaying && optionsChanged)
            {
                UpdateInstancedControls(property);
            }

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUIUtility.fieldWidth = previousFieldWidth;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            return EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightControlOptionsProperty, controlOptionsElementContent) + EditorGUI.GetPropertyHeight(currentProperty.ControlOptionGroupDataProperty) + (EditorUtils.LineHeight + EditorUtils.VerticalBuffer + EditorUtils.VerticalSpace) * 2f;
        }

        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    ControlOptionGroupDataProperty = property.FindPropertyRelative("controlOptionGroupData"),
                    LightControlOptionsProperty = property.FindPropertyRelative("lightControlOptions")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }

        private LightControllerGroup FindInstancedSelf(SerializedProperty self)
        {
            string propertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups");
            Object propertyContainer = self.serializedObject.targetObject;
            LightControllerGroup instancedSelf = ReflectionUtils.GetMemberAtPath<LightControllerGroup>(propertyContainer, propertyPath);

            return instancedSelf;
        }

        private void UpdateInstancedControls(SerializedProperty self)
        {
            self.serializedObject.ApplyModifiedProperties();

            LightControllerGroup instancedVersionOfSelf = FindInstancedSelf(self);
            InstancedControlOption[] controlOptions = ReflectionUtils.GetMemberAtPath<InstancedControlOption[]>(instancedVersionOfSelf, "controlOptions");
            LightControlOption[] currentOptions = ReflectionUtils.GetMemberAtPath<LightControlOption[]>(instancedVersionOfSelf, "controllerGroupData.lightControlOptions");

            controlOptions = controlOptions
                .Where(control => control != null && currentOptions
                    .Any(option => option != null && EditorUtils.InstancedVersionOf(control, option)))
                .Concat(currentOptions
                    .Where(control => control != null && !controlOptions
                        .Any(option => option != null && EditorUtils.InstancedVersionOf(option, control)))
                    .Select(control => control.GetInstanced()))
                .ToArray();
            
            bool saveLightColor = !controlOptions.Any(option => option != null && option.ApplyOn(ApplicationStages.LightColorApplication));
            bool saveMaterialColor = !controlOptions.Any(option => option != null && option.ApplyOn(ApplicationStages.MaterialColorApplication));

            ReflectionUtils.SetMemberAtPath(instancedVersionOfSelf, saveLightColor, "controlOptionGroup.saveLightColor");
            ReflectionUtils.SetMemberAtPath(instancedVersionOfSelf, saveMaterialColor, "controlOptionGroup.saveMaterialColor");

            self.serializedObject.Update();
        }
    }
}

#endif