#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightControls.Utilities
{
    [InitializeOnLoad]
    public class StagerReorderableListWrapper
    {
        private const double doubleClickTimeFrame = 0.5;

        //private static Dictionary<string, List<bool>> toggleSets;
        private static Texture2D alternateBoxTexture;
        private static GUIStyle headingStyle;

        public float ListHeight
        {
            get
            {
                float elementHeight = 0;

                for (int i = 0; i < displayList.count; i++)
                {
                    elementHeight += GetElementHeight(i);
                }

                return elementHeight + displayList.headerHeight + displayList.footerHeight + 5f;
            }
        }
        
        private ReorderableList displayList;

        private SerializedProperty selectedCollapsed => displayList
            .serializedProperty
            .GetArrayElementAtIndex(currentFocus)
            .FindPropertyRelative("serializedInspectorInfo.Collapsed");
        
        private int currentFocus;
        private double clickStartTime;

        static StagerReorderableListWrapper()
        {
            alternateBoxTexture = Resources.Load<Texture2D>("Textures/UI/Custom Box Texture");
            headingStyle = new GUIStyle()
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
        }

        public StagerReorderableListWrapper(SerializedObject obj, SerializedProperty listProperty)
        {
            displayList = new ReorderableList(obj, listProperty);
            
            clickStartTime = EditorApplication.timeSinceStartup;

            displayList.draggable = true;
            displayList.showDefaultBackground = false;

            displayList.drawHeaderCallback = DrawHeader;
            displayList.drawElementCallback = DrawElement;
            displayList.drawElementBackgroundCallback = DrawElementBackground;
            displayList.elementHeightCallback = GetElementHeight;
            displayList.onMouseUpCallback = SelectElement;
            displayList.onChangedCallback = OnListChange;
            displayList.onReorderCallback = OnReorder;
        }
        
        public void DisplayList(Rect rect)
        {
            displayList.DoList(rect);
        }
        
        private void DrawHeader(Rect rect)
        {
            //EditorGUI.LabelField(rect, new GUIContent("Stages"));
        }
        
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = displayList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty elementName = element.FindPropertyRelative("serializedInspectorInfo.Name");
            SerializedProperty elementCollapsed = element.FindPropertyRelative("serializedInspectorInfo.Collapsed");

            bool isDefaultName = string.IsNullOrWhiteSpace(elementName.stringValue) || elementName.stringValue.StartsWith("Stage");
            string stageNumberHeading = string.Format("Stage {0}", index + 1);
            string nameHeading = isDefaultName
                ? stageNumberHeading
                : elementName.stringValue;
            string numberReferenceHeading = isDefaultName
                ? string.Empty
                : string.Format("({0})", stageNumberHeading);

            GUIContent nameContent = EditorGUIUtility.TrTempContent(nameHeading);
            Vector2 nameSize = headingStyle.CalcSize(nameContent);
            Rect headingRect = EditorUtils.GetTopRect(rect, nameSize.y);
            rect.y = headingRect.yMax + EditorUtils.VerticalSpace;

            float labelOffset = 5f;
            Rect nameRect = new Rect(headingRect.position, nameSize);
            Rect labelRect = new Rect(headingRect.x + nameSize.x + labelOffset, headingRect.y, headingRect.width - nameSize.x - labelOffset, headingRect.height);
            
            elementName.stringValue = EditorGUI.TextField(nameRect, nameHeading, headingStyle);
            EditorGUI.LabelField(labelRect, numberReferenceHeading, headingStyle);
            
            if(!elementCollapsed.boolValue)
                EditorGUI.PropertyField(rect, element);
        }

        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index == displayList.count - 1)
                rect.yMax += 2f;
            
            if (!isFocused)
            {
                GUI.Box(rect, GUIContent.none);
            }
            else
            {
                Rect topBorderTextureCoordinates = new Rect(0f, 0f, alternateBoxTexture.width, 1f);
                Rect bottomBorderTextureCoordinates = new Rect(0f, alternateBoxTexture.height - 1, alternateBoxTexture.width, 1f);
                Rect topBorderGUICoordinates = new Rect(rect.x, rect.y, rect.width, 1f);
                Rect bottomBorderGUICoordinates = new Rect(rect.x, rect.yMax - 1f, rect.width, 1f);
                
                GUI.DrawTexture(rect, alternateBoxTexture, ScaleMode.StretchToFill);
                GUI.DrawTextureWithTexCoords(topBorderGUICoordinates, alternateBoxTexture, topBorderTextureCoordinates);
                GUI.DrawTextureWithTexCoords(bottomBorderGUICoordinates, alternateBoxTexture, bottomBorderTextureCoordinates);
            }
        }
        
        private void SelectElement(ReorderableList list)
        {
            if (currentFocus == list.index)
            {
                if (EditorApplication.timeSinceStartup - clickStartTime > doubleClickTimeFrame)
                {
                    clickStartTime = EditorApplication.timeSinceStartup;
                }
                else
                {
                    displayList.index = -1;
                    displayList.ReleaseKeyboardFocus();
                    selectedCollapsed.boolValue = !selectedCollapsed.boolValue;
                }
            }
            else
            {
                currentFocus = list.index;
                clickStartTime = EditorApplication.timeSinceStartup;
            }
        }

        private void OnListChange(ReorderableList list)
        {
            int i = 0;
        }

        private void OnReorder(ReorderableList list)
        {
            int i = 0;
        }

        private float GetElementHeight(int index)
        {
            SerializedProperty element = displayList
                .serializedProperty
                .GetArrayElementAtIndex(index)
                .FindPropertyRelative("serializedInspectorInfo.Collapsed");

            float elementPropertyHeight = !element.boolValue
                ? EditorGUI.GetPropertyHeight(displayList.serializedProperty.GetArrayElementAtIndex(index))
                : 0f;
            
            return elementPropertyHeight + headingStyle.CalcSize(GUIContent.none).y + EditorUtils.VerticalSpace;
        }
    }
}

#endif