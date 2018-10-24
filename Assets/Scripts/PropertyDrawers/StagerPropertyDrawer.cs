#if UNITY_EDITOR

using LightControls.ControlOptions.Stages;
using LightControls.Utilities;

using UnityEditor;

using UnityEngine;

namespace LightControls.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Stager))]
    public class StagerPropertyDrawer : PropertyDrawer
    {
        private readonly GUIContent useStagesContent = new GUIContent("Use Stages", "");
        private readonly GUIContent randomizeStageOrderContent = new GUIContent("Randomize Stage Order", "");
        private readonly GUIContent allowLooseOrderContent = new GUIContent("Allow Loose Order", "");

        private readonly GUIContent controlOptionsLengthContent = new GUIContent("Control Options", "");
        private readonly GUIContent controlOptionsElementContent = new GUIContent("Control Option", "");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(position, label, property);

            PropertyDrawerRect currentRect = new PropertyDrawerRect(position);

            using (EditorUtils.StartIndent(currentRect, indent))
            {
                EditorGUI.BeginChangeCheck();

                currentRect.CurrentRect = EditorUtils.GetTopRect(currentRect.CurrentRect, EditorUtils.LineHeight);
                property.FindPropertyRelative("RandomizeNextStage").boolValue = EditorGUI.Toggle(currentRect.CurrentRect, randomizeStageOrderContent, property.FindPropertyRelative("RandomizeNextStage").boolValue);
                //EditorGUI.PropertyField(currentRect.CurrentRect, property.FindPropertyRelative("RandomizeNextStage"), randomizeStageOrderContent);

                currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);
                property.FindPropertyRelative("CanSkipStages").boolValue = EditorGUI.Toggle(currentRect.CurrentRect, allowLooseOrderContent, property.FindPropertyRelative("CanSkipStages").boolValue);

                //currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);
                //int size = EditorUtils.ArraySizeField(currentRect.CurrentRect, new GUIContent("Size"), self.StageControl.Stages.Length);

                //if (EditorGUI.EndChangeCheck())
                //{
                //    self.Stages = MiscUtils.ResizeAndFillWith(self.Stages, size, () => null);
                //    self.ControlOptionInfos = MiscUtils.ResizeAndFill(self.ControlOptionInfos, size);
                //}

                //parent.Update();

                //for (int i = 0; i < self.StageControl.Stages.Length; i++)
                //{
                //    SerializedProperty stage = property.FindPropertyRelative("Stages").GetArrayElementAtIndex(i);

                //    SerializedProperty controlOptionInfo = property.FindPropertyRelative("ControlOptionInfos").GetArrayElementAtIndex(i);

                //    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorUtils.LineHeight);

                //    Rect labelRect = currentRect.CurrentRect;

                //    EditorGUI.LabelField(labelRect, new GUIContent(EditorUtils.GetRichText(string.Format("Stage {0}", i + 1), TextOptions.Bold), ""), EditorUtils.RichTextStyle);

                //    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorGUI.GetPropertyHeight(stage));

                //    Rect full = currentRect.CurrentRect;

                //    currentRect.CurrentRect = EditorUtils.GetRectBelow(currentRect.CurrentRect, EditorGUI.GetPropertyHeight(controlOptionInfo));

                //    Rect full2 = currentRect.CurrentRect;

                //    Rect total = new Rect(full.x, full.y, full.width, full.height + full2.height + EditorUtils.VerticalBuffer * 2);

                //    GUI.Box(total, GUIContent.none);

                //    EditorGUI.PropertyField(full, stage);

                //    parent.ApplyModifiedProperties();

                //    self.StageControl.Stages[i].ControlOptions = self.StageControl.Stages[i].ControlOptions.Select(option => option == null ? UnityEngine.Object.Instantiate(GetInstancedOption()) : option).ToArray();

                //    parent.Update();

                //    EditorGUI.PropertyField(full2, controlOptionInfo);

                //    parent.ApplyModifiedProperties();

                //    self.StageControl.ControlOptionInfos = self.StageControl.ControlOptionInfos.Select(info => info == null ? new ControlOptionInfo() : info).ToArray();

                //    parent.Update();
                //}
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorUtils.LineHeight + EditorUtils.VerticalBuffer) * 2f;
        }
    }
}

#endif