#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightControls.Utilities
{
    //[InitializeOnLoad]
    //public class UniqueDataTrackerLoader
    //{
    //    static UniqueDataTrackerLoader()
    //    {

    //    }
    //}

    [InitializeOnLoad]
    public class StagerReorderableListWrapper
    {
        private const double doubleClickTimeFrame = 0.5;
        
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
        private List<StageData> stageData;
        
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
            stageData = new List<StageData>();

            var valueTracker = new List<string>();

            for(int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty elementGUID = GUIDAt(i);

                if (string.IsNullOrWhiteSpace(elementGUID.stringValue) || valueTracker.Contains(elementGUID.stringValue))
                    elementGUID.stringValue = GUID.Generate().ToString();

                valueTracker.Add(elementGUID.stringValue);

                StageData data = GetValueAt(i);
                
                stageData.Add(data);
            }

            clickStartTime = EditorApplication.timeSinceStartup;

            displayList.draggable = true;
            displayList.showDefaultBackground = false;

            displayList.drawHeaderCallback = DrawHeader;
            displayList.drawElementCallback = DrawElement;
            displayList.drawElementBackgroundCallback = DrawElementBackground;
            displayList.elementHeightCallback = GetElementHeight;
            displayList.onMouseUpCallback = SelectElement;
            displayList.onRemoveCallback = RemoveElement;
            displayList.onAddCallback = AddElement;
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

            StageData data = stageData[index];

            bool isDefaultName = string.IsNullOrWhiteSpace(data.Name) || data.Name.StartsWith("Stage");
            string stageNumberHeading = string.Format("Stage {0}", index + 1);
            string nameHeading = isDefaultName
                ? stageNumberHeading
                : data.Name;
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

            data.Name = EditorGUI.TextField(nameRect, nameHeading, headingStyle);

            EditorGUI.LabelField(labelRect, numberReferenceHeading, headingStyle);
            
            if(!data.Collapsed)
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

        private void AddElement(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);

            SerializedProperty elementGUID = GUIDAt(list.count - 1);
            elementGUID.stringValue = GUID.Generate().ToString();

            StageData data = GetValueAt(list.count - 1);
            stageData.Add(data);
        }

        private void RemoveElement(ReorderableList list)
        {
            UniqueDataTracker.Singleton.Data.RemoveData(GUIDAt(list.index).stringValue);

            stageData.Remove(stageData[list.index]);

            ReorderableList.defaultBehaviours.DoRemoveButton(list);
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
                    
                    stageData[currentFocus].Collapsed = !stageData[currentFocus].Collapsed;
                }
            }
            else
            {
                currentFocus = list.index;
                clickStartTime = EditorApplication.timeSinceStartup;
            }
        }
        
        private float GetElementHeight(int index)
        {
            float elementPropertyHeight = !stageData[index].Collapsed
                ? EditorGUI.GetPropertyHeight(displayList.serializedProperty.GetArrayElementAtIndex(index))
                : 0f;
            
            return elementPropertyHeight + headingStyle.CalcSize(GUIContent.none).y + EditorUtils.VerticalSpace;
        }

        private StageData GetValueAt(int index)
        {
            SerializedProperty elementGUID = GUIDAt(index);

            StageData data = UniqueDataTracker.Singleton.Data.GetOrCreateData<StageData>(elementGUID.stringValue);

            return data;
        }

        private SerializedProperty GUIDAt(int index)
        {
            return displayList
                .serializedProperty
                .GetArrayElementAtIndex(index)
                .FindPropertyRelative("guid");
        }
    }
}

#endif