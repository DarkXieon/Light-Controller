#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using LightControls.Controllers;
using LightControls.Controllers.Data;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ControlOptionGroupData))]
    public class ControlOptionGroupDataDrawer : PropertyDrawer
    {
        private static readonly GUIContent lightArrayLengthContent;
        private static readonly GUIContent rendererArrayLengthContent;
        private static readonly GUIContent lightArrayElementContent;
        private static readonly GUIContent rendererArrayElementContent;

        static ControlOptionGroupDataDrawer()
        {
            lightArrayLengthContent = new GUIContent("Lights", "");
            rendererArrayLengthContent = new GUIContent("Material Renderers", "");
            lightArrayElementContent = new GUIContent("Light", "");
            rendererArrayElementContent = new GUIContent("Renderer", "");
        }

        private class PropertyData
        {
            public SerializedProperty LightsProperty;
            public SerializedProperty RenderersProperty;
        }

        private Dictionary<string, PropertyData> currentPropertyPerPath = new Dictionary<string, PropertyData>();
        private PropertyData currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            Light[] initialLights = currentProperty.LightsProperty.GetArray<Light>();
            Renderer[] initialRenderers = currentProperty.RenderersProperty.GetArray<Renderer>();
            Material[][] initialMaterials = GetMaterialArray(initialRenderers);

            bool lightsChanged = false;
            bool renderersChanged = false;

            int indent = EditorGUI.indentLevel;
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            float previousFieldWidth = EditorGUIUtility.fieldWidth;
            
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.numberField);
            currentProperty.LightsProperty.arraySize = EditorUtils.ArraySizeField(indentedPosition, lightArrayLengthContent, currentProperty.LightsProperty.arraySize);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightsProperty, lightArrayElementContent));
            EditorGUI.BeginChangeCheck();
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, currentProperty.LightsProperty, lightArrayElementContent);
            lightsChanged = EditorGUI.EndChangeCheck();

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.VerticalSpace);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.numberField);
            currentProperty.RenderersProperty.arraySize = EditorUtils.ArraySizeField(indentedPosition, rendererArrayLengthContent, currentProperty.RenderersProperty.arraySize);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightsProperty, rendererArrayElementContent));
            EditorGUI.BeginChangeCheck();
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, currentProperty.RenderersProperty, rendererArrayElementContent);
            renderersChanged = EditorGUI.EndChangeCheck();

            if(EditorApplication.isPlaying && (lightsChanged || renderersChanged))
            {
                ControlOptionGroup instancedSelf = FindInstancedSelf(property);

                if(lightsChanged)
                {
                    UpdateInstancedLightColors(instancedSelf, initialLights);
                }

                if(renderersChanged)
                {
                    UpdateInstancedMaterialColors(instancedSelf, initialRenderers, initialMaterials);
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUIUtility.fieldWidth = previousFieldWidth;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            return EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightsProperty, lightArrayElementContent) + EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.RenderersProperty, rendererArrayElementContent) + (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f + EditorUtils.VerticalBuffer * 2f + EditorUtils.VerticalSpace;// * EditorGUIUtility.pixelsPerPoint);
        }
        
        private void Initialize(SerializedProperty property)
        {
            if (!currentPropertyPerPath.TryGetValue(property.propertyPath, out currentProperty))
            {
                currentProperty = new PropertyData()
                {
                    LightsProperty = property.FindPropertyRelative("lights"),
                    RenderersProperty = property.FindPropertyRelative("emissiveMaterialRenderers")
                };

                currentPropertyPerPath.Add(property.propertyPath, currentProperty);
            }
        }

        private ControlOptionGroup FindInstancedSelf(SerializedProperty self)
        {
            string propertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups").Replace("lightControllerGroupsData", "controlOptionGroup");
            UnityEngine.Object propertyContainer = self.serializedObject.targetObject;
            ControlOptionGroup instancedSelf = ReflectionUtils.GetMemberAtPath<ControlOptionGroup>(propertyContainer, propertyPath);

            return instancedSelf;
        }

        private void UpdateInstancedLightColors(ControlOptionGroup instancedSelf, Light[] previousLights)
        {
            Color[] lightColors = ReflectionUtils.GetMemberAtPath<Color[]>(instancedSelf, "lightColors");
            Light[] currentLights = currentProperty.LightsProperty.GetArray<Light>();
            Array.Resize(ref lightColors, currentProperty.LightsProperty.arraySize);

            for(int i = 0; i < lightColors.Length; i++)
            {
                lightColors[i] = i < previousLights.Length && currentLights[i] == previousLights[i]
                    ? lightColors[i]
                    : currentLights[i].color;
            }
        }

        private void UpdateInstancedMaterialColors(ControlOptionGroup instancedSelf, Renderer[] previousRenderers, Material[][] previousMaterials)
        {
            Color[][] materialColors = ReflectionUtils.GetMemberAtPath<Color[][]>(instancedSelf, "materialColors");
            Renderer[] currentRenderers = currentProperty.RenderersProperty.GetArray<Renderer>();
            Material[][] currentMaterials = GetMaterialArray(currentRenderers);
            Array.Resize(ref materialColors, currentMaterials.Length);

            for(int i = 0; i < currentMaterials.Length; i++)
            {
                Array.Resize(ref materialColors[i], currentMaterials[i].Length);

                bool sameRenderer = i < previousRenderers.Length && currentRenderers[i] == previousRenderers[i];

                for(int k = 0; k < currentMaterials[i].Length; k++)
                {
                    bool sameMaterial = k < previousMaterials[i].Length && previousMaterials[i][k] == currentMaterials[i][k];

                    if(!sameRenderer || !sameMaterial)
                    {
                        currentMaterials[i][k].EnableKeyword("_EMISSION");
                        materialColors[i][k] = currentMaterials[i][k].GetColor("_EmissionColor");
                    }
                }

                if(!sameRenderer)
                {
                    currentRenderers[i].UpdateGIMaterials();
                }
            }
        }

        private Material[][] GetMaterialArray(Renderer[] renderers)
        {
            Material[][] materials = new Material[renderers.Length][];

            for(int i = 0; i < renderers.Length; i++)
            {
                int length = renderers[i] != null
                    ? renderers[i].sharedMaterials.Length
                    : 0;

                materials[i] = new Material[length];

                for(int k = 0; k < materials[i].Length; k++)
                {
                    materials[i][k] = renderers[i].sharedMaterials[k];
                }
            }

            return materials;
        }
    }
}

#endif