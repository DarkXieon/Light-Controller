#if UNITY_EDITOR

using System.Linq;

using LightControls.Controllers;
using LightControls.ControlOptions;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(LightControlInfo))]
    public class LightControlInfoDrawer : PropertyDrawer
    {
        private readonly GUIContent useStagesContent = new GUIContent("Use Stages","");
        private readonly GUIContent randomizeStageOrderContent = new GUIContent("Randomize Stage Order", "");
        private readonly GUIContent allowLooseOrderContent = new GUIContent("Allow Loose Order", ""); 

        private readonly GUIContent controlOptionsLengthContent = new GUIContent("Control Options", "");
        private readonly GUIContent controlOptionsElementContent = new GUIContent("Control Option", "");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedObject parent = property.serializedObject;
            LightControlInfo self = ReflectionUtils.GetMemberAtPath<LightControlInfo>(parent.targetObject, property.propertyPath);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(position, label, property);

            PropertyDrawerRect currentRect = new PropertyDrawerRect(position);

            using (EditorUtils.StartIndent(currentRect, indent))
            {
                //SerializedProperty useStages = property.FindPropertyRelative("UseStages");

                currentRect.CurrentRect = EditorUtils.GetTopRect(currentRect.CurrentRect, EditorUtils.LineHeight);

                //useStages.boolValue = EditorGUI.Toggle(currentRect.CurrentRect, useStagesContent, useStages.boolValue);

                //parent.ApplyModifiedProperties();

                //if (useStages.boolValue)
                //{
                //    EditorGUI.BeginChangeCheck();

                //    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);
                //    property.FindPropertyRelative("StageControl.RandomizeNextStage").boolValue = EditorGUI.Toggle(currentRect.CurrentRect, randomizeStageOrderContent, property.FindPropertyRelative("StageControl.RandomizeNextStage").boolValue);

                //    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);
                //    property.FindPropertyRelative("StageControl.CanSkipStages").boolValue = EditorGUI.Toggle(currentRect.CurrentRect, allowLooseOrderContent, property.FindPropertyRelative("StageControl.CanSkipStages").boolValue);

                //    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);
                //    int size = EditorUtils.ArraySizeField(currentRect.CurrentRect, new GUIContent("Size"), self.StageControl.Stages.Length);

                //    if (EditorGUI.EndChangeCheck())
                //    {
                //        self.StageControl.Stages = MiscUtils.ResizeAndFillWith(self.StageControl.Stages, size, () => null);
                //        self.StageControl.ControlOptionInfos = MiscUtils.ResizeAndFill(self.StageControl.ControlOptionInfos, size);
                //    }

                //    parent.Update();

                //    for (int i = 0; i < self.StageControl.Stages.Length; i++)
                //    {
                //        SerializedProperty stage = property.FindPropertyRelative("StageControl.Stages").GetArrayElementAtIndex(i);

                //        SerializedProperty controlOptionInfo = property.FindPropertyRelative("StageControl.ControlOptionInfos").GetArrayElementAtIndex(i);

                //        currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);

                //        Rect labelRect = currentRect.CurrentRect;

                //        EditorGUI.LabelField(labelRect, new GUIContent(EditorUtils.GetRichText(string.Format("Stage {0}", i + 1), TextOptions.Bold), ""), EditorUtils.RichTextStyle);

                //        currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorGUI.GetPropertyHeight(stage));

                //        Rect full = currentRect.CurrentRect;

                //        currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorGUI.GetPropertyHeight(controlOptionInfo));

                //        Rect full2 = currentRect.CurrentRect;

                //        Rect total = new Rect(full.x, full.y, full.width, full.height + full2.height + EditorUtils.VerticalBuffer * 2);
                        
                //        GUI.Box(total, GUIContent.none);

                //        EditorGUI.PropertyField(full, stage);

                //        parent.ApplyModifiedProperties();

                //        self.StageControl.Stages[i].ControlOptions = self.StageControl.Stages[i].ControlOptions.Select(option => option == null ? UnityEngine.Object.Instantiate(GetInstancedOption()) : option).ToArray();

                //        parent.Update();

                //        EditorGUI.PropertyField(full2, controlOptionInfo);

                //        parent.ApplyModifiedProperties();

                //        self.StageControl.ControlOptionInfos = self.StageControl.ControlOptionInfos.Select(info => info == null ? new ControlOptionInfo() : info).ToArray();

                //        parent.Update();
                //    }
                //}
                //else
                //{
                    SerializedProperty controlOptionInfo = property.FindPropertyRelative("ControlOptionInfo");
                    
                    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorGUI.GetPropertyHeight(controlOptionInfo));
                    
                    EditorGUI.PropertyField(currentRect.CurrentRect, controlOptionInfo);

                    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);

                    int size = EditorUtils.ArraySizeField(currentRect.CurrentRect, controlOptionsLengthContent, self.LightControlOptions.Length);

                self.LightControlOptions = MiscUtils.ResizeAndFillWith(self.LightControlOptions, size, () => null);

                    //self.LightControlOptions = MiscUtils.ResizeAndFillWith(self.LightControlOptions, size, () =>
                    //{
                    //    var instance = UnityEngine.Object.Instantiate(ScriptableObject.CreateInstance<InstancedControlOption>());

                //    return instance;
                //});

                parent.Update();

                using (EditorUtils.StartIndent(currentRect))
                    {
                        SerializedProperty controlOptionsProperty = property.FindPropertyRelative("LightControlOptions");

                        //InstancedControlOption[] previousOptions = controlOptionsProperty.GetArray<InstancedControlOption>();
                    
                        currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.GetSinglelinePropertyArrayHeight(controlOptionsProperty, controlOptionsElementContent));

                        //EditorGUI.BeginChangeCheck();

                        EditorUtils.DisplaySinglelinePropertyArray(currentRect.CurrentRect, controlOptionsProperty, controlOptionsElementContent);

                        parent.ApplyModifiedProperties();

                        //self.LightControlOptions = self.LightControlOptions.Select(option => option == null ? UnityEngine.Object.Instantiate(GetInstancedOption()) : option).ToArray();

                        parent.Update();
                    //if (EditorGUI.EndChangeCheck())
                    //{

                    //    parent.ApplyModifiedProperties();

                    //    InstancedControlOption[] currentOptions = controlOptionsProperty.GetArray<InstancedControlOption>();

                    //    List<InstancedControlOption> changedOptions = new List<InstancedControlOption>();

                    //    for(int i = 0; i < previousOptions.Length; i++)
                    //    {
                    //        if(previousOptions[i].ClonedOption != currentOptions[i].ClonedOption)
                    //        {
                    //            if(self.LightControlOptions[i].ClonedOption is StagedControlOption)
                    //            {
                    //                ((StagedControlOption)self.LightControlOptions[i].ClonedOption).
                    //            }

                    //        }
                    //    }
                    //}
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //float size = 0f;

            //SerializedProperty useStages = property.FindPropertyRelative("UseStages");

            //if(useStages.boolValue)
            //{
            //    SerializedProperty stages = property.FindPropertyRelative("StageControl.Stages");
            //    SerializedProperty controlOptionInfos = property.FindPropertyRelative("StageControl.ControlOptionInfos");
                
            //    float stagesSize = stages
            //        .GetArray<SerializedProperty>()
            //        .Select(prop => EditorGUI.GetPropertyHeight(prop))
            //        .Sum();

            //    float optionInfoSize = controlOptionInfos
            //        .GetArray<SerializedProperty>()
            //        .Select(prop => EditorGUI.GetPropertyHeight(prop))
            //        .Sum();

            //    SerializedObject parent = property.serializedObject;
            //    LightControlInfo self = ReflectionUtils.GetMemberAtPath<LightControlInfo>(parent.targetObject, property.propertyPath);

            //    var test = self.StageControl.ControlOptionInfos;

            //    return stagesSize + optionInfoSize + (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * (4f + stages.arraySize);
            //}
            //else
            //{
                SerializedProperty controlOptionInfo = property.FindPropertyRelative("ControlOptionInfo");
                SerializedProperty controlOptions = property.FindPropertyRelative("LightControlOptions");

                return EditorUtils.GetSinglelinePropertyArrayHeight(controlOptions, controlOptionsElementContent) + EditorGUI.GetPropertyHeight(controlOptionInfo) + (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f;
            //}
        }

        //private InstancedControlOption GetInstancedOption()
        //{
        //    InstancedControlOption defaultValue = ScriptableObject.CreateInstance<InstancedControlOption>();

        //    ReflectionUtils.SetMemberAtPath(defaultValue, null, "originalOption");
        //    ReflectionUtils.SetMemberAtPath(defaultValue, null, "clonedOption");

        //    return defaultValue;
        //}
    }
}

#endif