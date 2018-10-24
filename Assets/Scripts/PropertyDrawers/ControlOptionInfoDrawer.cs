#if UNITY_EDITOR

using System;
using System.Linq;

using LightControls.Controllers;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ControlOptionInfo))]
    public class ControlOptionInfoDrawer : PropertyDrawer
    {
        private readonly GUIContent lightArrayLengthContent = new GUIContent("Lights", "");
        private readonly GUIContent rendererArrayLengthContent = new GUIContent("Material Renderers", "");

        private readonly GUIContent lightArrayElementContent = new GUIContent("Light", "");
        private readonly GUIContent rendererArrayElementContent = new GUIContent("Renderer", "");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedObject parent = property.serializedObject;
            ControlOptionInfo self = ReflectionUtils.GetMemberAtPath<ControlOptionInfo>(parent.targetObject, property.propertyPath);
            
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(position, label, property);

            PropertyDrawerRect currentRect = new PropertyDrawerRect(position);

            using (EditorUtils.StartIndent(currentRect, indent))
            {
                currentRect.CurrentRect = EditorUtils.GetTopRect(currentRect.CurrentRect, EditorStyles.numberField);
                
                int size = EditorUtils.ArraySizeField(currentRect.CurrentRect, lightArrayLengthContent, self.Lights.Length);

                self.Lights = MiscUtils.ResizeAndFill(self.Lights, size);

                parent.Update();

                using (EditorUtils.StartIndent(currentRect))
                {
                    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.GetSinglelinePropertyArrayHeight(property.FindPropertyRelative("Lights"), lightArrayElementContent));

                    EditorGUI.BeginChangeCheck();

                    EditorUtils.DisplaySinglelinePropertyArray(currentRect.CurrentRect, property.FindPropertyRelative("Lights"), lightArrayElementContent, typeof(Light));

                    if(EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        Color[] currentColors = property.FindPropertyRelative("LightColors").GetArray<Color>();
                        Light[] lights = property.FindPropertyRelative("Lights").GetArray<Light>();

                        Array.Resize(ref currentColors, size);

                        for(int i = 0; i < currentColors.Length; i++)
                        {
                            if(currentColors[i] == default(Color) && lights[i] != null)
                            {
                                currentColors[i] = lights[i].color;
                            }
                        }

                        property.FindPropertyRelative("LightColors").SetArray(currentColors.Cast<object>().ToArray());
                    }

                    parent.ApplyModifiedProperties();
                }

                currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);
                size = EditorUtils.ArraySizeField(currentRect.CurrentRect, rendererArrayLengthContent, self.EmissiveMaterialRenderers.Length);

                self.EmissiveMaterialRenderers = MiscUtils.ResizeAndFill(self.EmissiveMaterialRenderers, size);
                parent.Update();

                using (EditorUtils.StartIndent(currentRect))
                {
                    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.GetSinglelinePropertyArrayHeight(property.FindPropertyRelative("EmissiveMaterialRenderers"), rendererArrayElementContent));//, property.FindPropertyRelative("EmissiveMaterialRenderers").arraySize);
                    //currentRect.CurrentRect = new Rect(currentRect.CurrentRect.x, currentRect.CurrentRect.y, currentRect.CurrentRect.width, EditorUtils.GetSinglelinePropertyArrayHeight(property.FindPropertyRelative("EmissiveMaterialRenderers"), rendererArrayElementContent));

                    EditorGUI.BeginChangeCheck();

                    EditorUtils.DisplaySinglelinePropertyArray(currentRect.CurrentRect, property.FindPropertyRelative("EmissiveMaterialRenderers"), rendererArrayElementContent, typeof(Renderer));
                    int t = property.FindPropertyRelative("EmissiveMaterialRenderers").arraySize;

                    bool materialsChanged = EditorGUI.EndChangeCheck();
                    materialsChanged = materialsChanged || property.FindPropertyRelative("EmissiveMaterialRenderers").arraySize != self.MaterialColors.Length;

                    int maxIndex = Mathf.Min(property.FindPropertyRelative("EmissiveMaterialRenderers").arraySize, self.MaterialColors.Length);

                    for (int i = 0; i < maxIndex && !materialsChanged; i++)
                    {
                        materialsChanged = property.FindPropertyRelative("EmissiveMaterialRenderers").GetArray<Renderer>()[i].materials.Length != self.MaterialColors[i].Length;
                    }

                    parent.ApplyModifiedProperties();

                    if (materialsChanged && EditorApplication.isPlaying)
                    {
                        Renderer[] renderers = property.FindPropertyRelative("EmissiveMaterialRenderers").GetArray<Renderer>();

                        if (self.MaterialColors.Length != renderers.Length)
                        {
                            Array.Resize(ref self.MaterialColors, renderers.Length);
                        }

                        for (int i = 0; i < renderers.Length; i++)
                        {
                            if (renderers[i] == null)
                            {
                                continue;
                            }

                            if (self.MaterialColors[i].Length != renderers[i].materials.Length)
                            {
                                Array.Resize(ref self.MaterialColors[i], renderers[i].materials.Length);
                            }

                            for (int k = 0; k < renderers[i].materials.Length; k++)
                            {
                                if (self.MaterialColors[i][k] == default(Color) && renderers[i].materials[k] != null)
                                {
                                    renderers[i].materials[k].EnableKeyword("_EMISSION");
                                    self.MaterialColors[i][k] = renderers[i].materials[k].GetColor("_EmissionColor");
                                }
                            }

                            renderers[i].UpdateGIMaterials();
                        }
                        //when I add serialized properties here, save the indexes that have not been changed yet to make this always work
                    }

                    parent.Update();
                }
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty lightArray = property.FindPropertyRelative("Lights");
            SerializedProperty rendererArray = property.FindPropertyRelative("EmissiveMaterialRenderers");

            return EditorUtils.GetSinglelinePropertyArrayHeight(lightArray, lightArrayElementContent) + EditorUtils.GetSinglelinePropertyArrayHeight(rendererArray, rendererArrayElementContent) + (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f + EditorUtils.VerticalBuffer * 2f;// * EditorGUIUtility.pixelsPerPoint);
        }
    }
}

#endif