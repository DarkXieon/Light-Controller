#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
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
            //Material[][] initialMaterials = GetMaterialArray(initialRenderers);

            bool lightsChanged = false;
            bool renderersChanged = false;
            
            EditorGUI.BeginProperty(position, label, property);

            Rect indentedPosition = EditorUtils.BeginPropertyDrawer(position);
            
            indentedPosition = EditorUtils.GetTopRect(indentedPosition, EditorStyles.numberField);
            EditorGUI.BeginChangeCheck();
            EditorUtils.ArraySizeField<Light>(indentedPosition, currentProperty.LightsProperty, lightArrayLengthContent);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightsProperty, lightArrayElementContent));
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, currentProperty.LightsProperty, lightArrayElementContent);
            lightsChanged = EditorGUI.EndChangeCheck();

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.VerticalSpace);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorStyles.numberField);
            EditorGUI.BeginChangeCheck();
            EditorUtils.ArraySizeField<Renderer>(indentedPosition, currentProperty.RenderersProperty, rendererArrayLengthContent);

            indentedPosition = EditorUtils.GetRectBelow(indentedPosition, EditorUtils.GetSinglelinePropertyArrayHeight(currentProperty.LightsProperty, rendererArrayElementContent));
            EditorUtils.DisplaySinglelinePropertyArray(indentedPosition, currentProperty.RenderersProperty, rendererArrayElementContent);
            renderersChanged = EditorGUI.EndChangeCheck();

            if(EditorApplication.isPlaying && (lightsChanged || renderersChanged))
            {
                //ControlOptionGroup instancedSelf = FindInstancedSelf(property);

                if(lightsChanged)
                {
                    //UpdateInstancedLightColors(instancedSelf, initialLights);
                    //UpdateInstancedLightColors(property);
                    UpdateInstancedLightColors(property, initialLights);
                }

                if(renderersChanged)
                {
                    //UpdateInstancedMaterialColors(instancedSelf, initialRenderers, initialMaterials);
                    UpdateInstancedMaterialColors(property, initialRenderers);
                }
            }

            EditorUtils.EndPropertyDrawer();
            
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

        //private ControlOptionGroup FindInstancedSelf(SerializedProperty self)
        //{
        //    string propertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups").Replace("controlOptionGroupData", "controlOptionGroup");
        //    UnityEngine.Object propertyContainer = self.serializedObject.targetObject;
        //    ControlOptionGroup instancedSelf = ReflectionUtils.GetMemberAtPath<ControlOptionGroup>(propertyContainer, propertyPath);

        //    return instancedSelf;
        //}

        //private void UpdateInstancedLightColors(SerializedProperty self)
        //{
        //    //self.serializedObject.ApplyModifiedProperties();
            
        //    string instancedPropertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups").Replace("controlOptionGroupData", "controlOptionGroup");
        //    UnityEngine.Object propertyContainer = self.serializedObject.targetObject;

        //    string serializedOptionsPropertyPath = $"{self.propertyPath}.lights";
        //    string instancedOptionsPropertyPath = $"{instancedPropertyPath}.lightColors";

        //    Light[] lights = ReflectionUtils.GetMemberAtPath<Light[]>(propertyContainer, serializedOptionsPropertyPath);
        //    Color[] colors = ReflectionUtils.GetMemberAtPath<Color[]>(propertyContainer, instancedOptionsPropertyPath);
            
        //    self.serializedObject.ApplyModifiedProperties(); //this makes the arrays that the method below will get are the updated versions of the above two arrays

        //    EditorUtils.UpdateInstancedArray<Light, Color>(
        //        serializedContainer: propertyContainer,
        //        instancedContainer: propertyContainer,
        //        serializedPath: serializedOptionsPropertyPath,
        //        instancedPath: instancedOptionsPropertyPath,
        //        iterationsPath: null,
        //        isInstancedVersion: (light, color) => Array.IndexOf(lights, light) == Array.IndexOf(colors, color),
        //        getInstancedVersion: light => light.color);
        //}

        private void UpdateInstancedLightColors(SerializedProperty self, Light[] previousLights)
        {
            self.serializedObject.ApplyModifiedProperties();

            string instancedPropertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups").Replace("controlOptionGroupData", "controlOptionGroup");
            UnityEngine.Object propertyContainer = self.serializedObject.targetObject;

            string lightsPropertyPath = $"{self.propertyPath}.lights";
            string colorsPropertyPath = $"{instancedPropertyPath}.lightColors";
            
            Color[] colors = ReflectionUtils.GetMemberAtPath<Color[]>(propertyContainer, colorsPropertyPath);
            
            EditorUtils.UpdateInstancedArray<Light, Color>(
                serializedContainer: propertyContainer,
                instancedContainer: propertyContainer,
                serializedPath: lightsPropertyPath,
                instancedPath: colorsPropertyPath,
                iterationsPath: null,
                isInstancedVersion: (light, color) =>
                {
                    int index = Array.IndexOf(previousLights, light);

                    return index > -1
                        ? color == colors[index] //Array.IndexOf(previousLights, light) == Array.IndexOf(colors, color)
                        : false;
                },
                getInstancedVersion: light => light.color);
        }

        //private void UpdateInstancedLightColors(ControlOptionGroup instancedSelf, Light[] previousLights)
        //{
        //    Color[] lightColors = ReflectionUtils.GetMemberAtPath<Color[]>(instancedSelf, "lightColors");
        //    Light[] currentLights = currentProperty.LightsProperty.GetArray<Light>();
        //    Array.Resize(ref lightColors, currentProperty.LightsProperty.arraySize);

        //    for (int i = 0; i < lightColors.Length; i++)
        //    {
        //        lightColors[i] = i < previousLights.Length && currentLights[i] == previousLights[i]
        //            ? lightColors[i]
        //            : currentLights[i].color;
        //    }
        //}

        private void UpdateInstancedMaterialColors(SerializedProperty self, Renderer[] previousRenderers)
        {
            string instancedPropertyPath = self.propertyPath.Replace("lightControllerGroupsData", "lightControllerGroups").Replace("controlOptionGroupData", "controlOptionGroup");
            UnityEngine.Object propertyContainer = self.serializedObject.targetObject;

            string renderersPropertyPath = $"{self.propertyPath}.emissiveMaterialRenderers";
            string colorsPropertyPath = $"{instancedPropertyPath}.materialColors";

            RendererColor[] colors = ReflectionUtils.GetMemberAtPath<RendererColor[]>(propertyContainer, colorsPropertyPath);

            EditorUtils.UpdateInstancedArray<Renderer, RendererColor>(
                serializedContainer: propertyContainer,
                instancedContainer: propertyContainer,
                serializedPath: renderersPropertyPath,
                instancedPath: colorsPropertyPath,
                iterationsPath: null,
                isInstancedVersion: (renderer, color) =>
                {
                    int index = Array.IndexOf(previousRenderers, renderer);
                    
                    return index > -1
                        ? color.Colors == colors[index].Colors
                        : false;
                },
                getInstancedVersion: renderer =>
                {
                    Color[] rendererColors = new Color[renderer.materials.Length];

                    for(int i = 0; i < rendererColors.Length; i++)
                    {
                        renderer.materials[i].EnableKeyword("_EMISSION");
                        rendererColors[i] = renderer.materials[i].GetColor("_EmissionColor");
                    }

                    return new RendererColor(rendererColors);
                });

            //Color[][] materialColors = ReflectionUtils.GetMemberAtPath<Color[][]>(instancedSelf, "materialColors");
            //Renderer[] currentRenderers = currentProperty.RenderersProperty.GetArray<Renderer>();
            //Material[][] currentMaterials = GetMaterialArray(currentRenderers);
            //Array.Resize(ref materialColors, currentMaterials.Length);

            //for (int i = 0; i < currentMaterials.Length; i++)
            //{
            //    Array.Resize(ref materialColors[i], currentMaterials[i].Length);

            //    bool sameRenderer = i < previousRenderers.Length && currentRenderers[i] == previousRenderers[i];

            //    for (int k = 0; k < currentMaterials[i].Length; k++)
            //    {
            //        bool sameMaterial = k < previousMaterials[i].Length && previousMaterials[i][k] == currentMaterials[i][k];

            //        if (!sameRenderer || !sameMaterial)
            //        {
            //            currentMaterials[i][k].EnableKeyword("_EMISSION");
            //            materialColors[i][k] = currentMaterials[i][k].GetColor("_EmissionColor");
            //        }
            //    }

            //    if (!sameRenderer)
            //    {
            //        currentRenderers[i].UpdateGIMaterials();
            //    }
            //}
        }
        
        //private void UpdateInstancedMaterialColors(ControlOptionGroup instancedSelf, Renderer[] previousRenderers, Material[][] previousMaterials)
        //{
        //    Color[][] materialColors = ReflectionUtils.GetMemberAtPath<Color[][]>(instancedSelf, "materialColors");
        //    Renderer[] currentRenderers = currentProperty.RenderersProperty.GetArray<Renderer>();
        //    Material[][] currentMaterials = GetMaterialArray(currentRenderers);
        //    Array.Resize(ref materialColors, currentMaterials.Length);

        //    for(int i = 0; i < currentMaterials.Length; i++)
        //    {
        //        Array.Resize(ref materialColors[i], currentMaterials[i].Length);

        //        bool sameRenderer = i < previousRenderers.Length && currentRenderers[i] == previousRenderers[i];

        //        for(int k = 0; k < currentMaterials[i].Length; k++)
        //        {
        //            bool sameMaterial = k < previousMaterials[i].Length && previousMaterials[i][k] == currentMaterials[i][k];

        //            if(!sameRenderer || !sameMaterial)
        //            {
        //                currentMaterials[i][k].EnableKeyword("_EMISSION");
        //                materialColors[i][k] = currentMaterials[i][k].GetColor("_EmissionColor");
        //            }
        //        }

        //        if(!sameRenderer)
        //        {
        //            currentRenderers[i].UpdateGIMaterials();
        //        }
        //    }
        //}

        //private Material[][] GetMaterialArray(Renderer[] renderers)
        //{
        //    Material[][] materials = new Material[renderers.Length][];

        //    for(int i = 0; i < renderers.Length; i++)
        //    {
        //        int length = renderers[i] != null
        //            ? renderers[i].sharedMaterials.Length
        //            : 0;

        //        materials[i] = new Material[length];

        //        for(int k = 0; k < materials[i].Length; k++)
        //        {
        //            materials[i][k] = renderers[i].sharedMaterials[k];
        //        }
        //    }

        //    return materials;
        //}
    }
}

#endif