#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using LightControls.ControlOptions.Stages;
using UnityEditor;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.SceneManagement;
using static LightControls.Utilities.EditorUtils;

namespace LightControls.Utilities
{
    [InitializeOnLoad]
    public class StagerReorderableListWrapper
    {
        private const double doubleClickTimeFrame = 0.5;
        
        private static readonly Texture2D alternateBoxTexture;
        private static readonly GUIStyle headingStyle;

        //private readonly string instancedStagerPropertyName;
        //private readonly string instancedStageArrayPropertyName;

        public delegate void InstancedAddHandler(SerializedProperty elementProperty);
        public delegate void InstancedRemoveHandler(SerializedProperty elementProperty, int index);
        public delegate void InstancedReorderHandler(SerializedProperty elementProperty, int oldIndex, int newIndex);
        
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

        private InstancedAddHandler instancedAddUpdater;
        private InstancedRemoveHandler instancedRemoveUpdater;
        private InstancedReorderHandler instancedReorderUpdater;

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

        public StagerReorderableListWrapper( 
            SerializedProperty listProperty,
            InstancedAddHandler addHandler,
            InstancedRemoveHandler removeHandler,
            InstancedReorderHandler reorderHandler)
            //, string stagerPropertyName, string stageArrayPropertyName)
        {
            displayList = new ReorderableList(listProperty.serializedObject, listProperty);
            stageData = new List<StageData>();

            instancedAddUpdater = addHandler;
            instancedRemoveUpdater = removeHandler;
            instancedReorderUpdater = reorderHandler;

            //instancedStageArrayPropertyName = stageArrayPropertyName;
            //instancedStagerPropertyName = stagerPropertyName;

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
            displayList.onReorderCallbackWithDetails = OnReorderElements;
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

            if(EditorApplication.isPlaying)
            {
                instancedAddUpdater.Invoke(list.serializedProperty);
            }
        }

        private void RemoveElement(ReorderableList list)
        {
            int index = list.index;

            UniqueDataTracker.Singleton.Data.RemoveData(GUIDAt(list.index).stringValue);

            stageData.Remove(stageData[list.index]);

            ReorderableList.defaultBehaviours.DoRemoveButton(list);

            if (EditorApplication.isPlaying)
            {
                instancedRemoveUpdater(list.serializedProperty, index);
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
                    
                    stageData[currentFocus].Collapsed = !stageData[currentFocus].Collapsed;
                }
            }
            else
            {
                currentFocus = list.index;
                clickStartTime = EditorApplication.timeSinceStartup;
            }
        }
        
        private void OnReorderElements(ReorderableList list, int oldIndex, int newIndex)
        {
            if (EditorApplication.isPlaying)
            {
                instancedReorderUpdater.Invoke(list.serializedProperty, oldIndex, newIndex);
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

        private class InstancedStageModifier<TStage, TInstanced> : IDisposable
            where TStage : Stage
            where TInstanced : InstancedStage
        {
            public TStage[] Stages;
            public TInstanced[] InstancedStages;
            public int[] Iterations;

            private object container;
            private object instancedContainer;
            private string stagesPath;
            private string instancedStagesPath;
            private string iterationsPath;

            public InstancedStageModifier(
                object container,
                object instancedContainer,
                string stagesPath,
                string instancedStagesPath,
                string iterationsPath)
            {

                this.Stages = ReflectionUtils.GetMemberAtPath<TStage[]>(container, stagesPath);
                this.InstancedStages = ReflectionUtils.GetMemberAtPath<TInstanced[]>(instancedContainer, instancedStagesPath);
                this.Iterations = ReflectionUtils.GetMemberAtPath<int[]>(instancedContainer, iterationsPath);

                this.container = container;
                this.instancedContainer = instancedContainer;
                this.stagesPath = stagesPath;
                this.instancedStagesPath = instancedStagesPath;
                this.iterationsPath = iterationsPath;
            }

            public void Dispose()
            {
                ReflectionUtils.SetMemberAtPath(this.instancedContainer, this.InstancedStages, instancedStagesPath);
                ReflectionUtils.SetMemberAtPath(this.instancedContainer, this.Iterations, iterationsPath);
            }
        }

        public static void DefaultUpdateInstancedAdd<TStage, TInstanced>(
            object container,
            object instancedContainer,
            string stagesPath, 
            string instancedStagesPath, 
            string iterationsPath,
            Func<TStage, TInstanced> createInstanced)
            where TStage : Stage
            where TInstanced : InstancedStage
        {
            using (var modifier = new InstancedStageModifier<TStage, TInstanced>(container, instancedContainer, stagesPath, instancedStagesPath, iterationsPath))
            {
                Debug.Assert(modifier.Stages.Length - modifier.InstancedStages.Length == 1);

                int iterationsMin = modifier.Iterations.Min();

                Array.Resize(ref modifier.InstancedStages, modifier.Stages.Length);
                Array.Resize(ref modifier.Iterations, modifier.Stages.Length);

                modifier.InstancedStages[modifier.InstancedStages.Length - 1] = createInstanced(modifier.Stages[modifier.Stages.Length - 1]);
                modifier.Iterations[modifier.Iterations.Length - 1] = iterationsMin;
            }
        }

        public static void DefaultInstancedRemove<TStage, TInstanced>(
            object container,
            object instancedContainer,
            int removedAt,
            string stagesPath,
            string instancedStagesPath,
            string iterationsPath)
            where TStage : Stage
            where TInstanced : InstancedStage
        {
            using (var modifier = new InstancedStageModifier<TStage, TInstanced>(container, instancedContainer, stagesPath, instancedStagesPath, iterationsPath))
            {
                Debug.Assert(modifier.Stages.Length - modifier.InstancedStages.Length == -1);

                modifier.InstancedStages[removedAt] = null;

                for (int i = removedAt + 1; i < modifier.InstancedStages.Length; i++)
                {
                    modifier.InstancedStages[i - 1] = modifier.InstancedStages[i];
                    modifier.Iterations[i - 1] = modifier.Iterations[i];
                }

                Array.Resize(ref modifier.InstancedStages, modifier.Stages.Length);
                Array.Resize(ref modifier.Iterations, modifier.Stages.Length);
            }
        }

        public static void DefaultInstancedReorder<TStage, TInstanced>(
            object container,
            object instancedContainer,
            int oldIndex,
            int newIndex,
            string stagesPath,
            string instancedStagesPath,
            string iterationsPath)
            where TStage : Stage
            where TInstanced : InstancedStage
        {
            using (var modifier = new InstancedStageModifier<TStage, TInstanced>(container, instancedContainer, stagesPath, instancedStagesPath, iterationsPath))
            {
                Debug.Assert(modifier.Stages.Length - modifier.InstancedStages.Length == 0 && oldIndex != newIndex);

                TInstanced moved = modifier.InstancedStages[oldIndex];

                if (oldIndex > newIndex)
                {
                    for (int k = oldIndex; k > newIndex; k--)
                    {
                        modifier.InstancedStages[k] = modifier.InstancedStages[k - 1];
                        modifier.Iterations[k] = modifier.Iterations[k - 1];
                    }
                }
                else
                {
                    for (int k = oldIndex; k < newIndex; k++)
                    {
                        modifier.InstancedStages[k] = modifier.InstancedStages[k + 1];
                        modifier.Iterations[k] = modifier.Iterations[k - 1];
                    }
                }

                modifier.InstancedStages[newIndex] = moved;
            }
        }
    }
}

#endif