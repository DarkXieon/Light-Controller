#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using LightControls.Controllers;
using LightControls.Controllers.Data;
using LightControls.ControlOptions;
using LightControls.Editors;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;
using static LightControls.Utilities.EditorUtils;

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
            
            EditorGUI.BeginProperty(position, label, property);
            
            Rect indentedPosition = EditorUtils.BeginPropertyDrawer(position);

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorGUI.GetPropertyHeight(currentProperty.ControlOptionGroupDataProperty));
            EditorGUI.PropertyField(indentedPosition, currentProperty.ControlOptionGroupDataProperty, true);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.VerticalSpace);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.numberField);
            EditorGUI.BeginChangeCheck();
            EditorUtils.ArraySizeField<LightControlOption>(indentedPosition, currentProperty.LightControlOptionsProperty, controlOptionsLengthContent);
            
            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightControlOptionsProperty, controlOptionsElementContent));
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, currentProperty.LightControlOptionsProperty, controlOptionsElementContent);
            optionsChanged = EditorGUI.EndChangeCheck();

            if(EditorApplication.isPlaying && optionsChanged)
            {
                UpdateInstancedControls(property);
            }
            
            EditorUtils.EndPropertyDrawer();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            return EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightControlOptionsProperty, controlOptionsElementContent) 
                + EditorGUI.GetPropertyHeight(currentProperty.ControlOptionGroupDataProperty) 
                + (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f 
                +  EditorUtils.VerticalSpace;
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

        //private LightControllerGroup FindInstancedSelf(SerializedProperty self)
        //{
        //    string propertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups");
        //    Object propertyContainer = self.serializedObject.targetObject;
        //    LightControllerGroup instancedSelf = ReflectionUtils.GetMemberAtPath<LightControllerGroup>(propertyContainer, propertyPath);

        //    return instancedSelf;
        //}

        private void UpdateInstancedControls(SerializedProperty self)
        {
            self.serializedObject.ApplyModifiedProperties();

            string instancedPropertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups");
            Object propertyContainer = self.serializedObject.targetObject;
            
            string serializedOptionsPropertyPath = $"{self.propertyPath}.lightControlOptions";
            string instancedOptionsPropertyPath = $"{instancedPropertyPath}.controlOptions";
            string iterationsPropertyPath = $"{instancedPropertyPath}.controlIterations";

            EditorUtils.UpdateInstancedArray<LightControlOption, InstancedControlOption>(
                serializedContainer: propertyContainer,
                instancedContainer: propertyContainer,
                serializedPath: serializedOptionsPropertyPath,
                instancedPath: instancedOptionsPropertyPath,
                iterationsPath: iterationsPropertyPath,
                isInstancedVersion: (option, instanced) => EditorUtils.InstancedVersionOf(instanced, option),
                getInstancedVersion: option => option.GetInstanced());

            string updateTogglePath = $"{instancedPropertyPath}.controlOptionGroup.UpdateColorInfo";

            ReflectionUtils.SetMemberAtPath(propertyContainer, true, updateTogglePath);
        }
    }
}

#endif