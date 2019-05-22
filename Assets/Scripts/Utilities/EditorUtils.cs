#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using LightControls.Controllers;
using LightControls.Controllers.Data;
using LightControls.ControlOptions;
using LightControls.ControlOptions.Stages;
using UnityEditor;

using UnityEngine;

namespace LightControls.Utilities
{
    public enum ValueGenerationMode
    {
        Constant,
        List,
        Curve,
        BetweenValues,
        BetweenCurves,
        None
    }

    public class PropertyDrawerRect : IDisposable
    {
        public Rect CurrentRect { get; set; }
        //public Rect AlignmentRect { get; set; }

        //private int additionalIndentAmount;
        //private int previousIndentAmount;
        //private float previousLabelWidth;
        //private float previousFieldWidth;

        public PropertyDrawerRect(Rect startingRect)
        {
            CurrentRect = startingRect;

            int indent = EditorGUI.indentLevel;
            //previousLabelWidth = EditorGUIUtility.labelWidth;
            //previousFieldWidth = EditorGUIUtility.fieldWidth;

            //initialIndentAmount = EditorGUI.indentLevel;
        }

        public static implicit operator Rect(PropertyDrawerRect reference)
        {
            return reference.CurrentRect;
        }
        
        //public void Indent()
        //{
        //    additionalIndentAmount++;

        //    EditorGUI.indentLevel = 
        //    AlignmentRect = 
        //}

        //public void Unindent()
        //{

        //}

        public void Dispose()
        {

        }
    }

    public class Indent : IDisposable
    {
        private PropertyDrawerRect indentedRect;
        private float indent;
        
        public Indent(PropertyDrawerRect toIndent, float indentAmount)
        {
            indentedRect = toIndent;
            indent = indentAmount;
            indentedRect.CurrentRect = Rect.MinMaxRect(indentedRect.CurrentRect.xMin + indentAmount, indentedRect.CurrentRect.yMin, indentedRect.CurrentRect.xMax, indentedRect.CurrentRect.yMax);

            EditorGUIUtility.labelWidth -= indentAmount;
        }

        public void Dispose()
        {
            indentedRect.CurrentRect = Rect.MinMaxRect(indentedRect.CurrentRect.xMin - indent, indentedRect.CurrentRect.yMin, indentedRect.CurrentRect.xMax, indentedRect.CurrentRect.yMax);

            EditorGUIUtility.labelWidth += indent;
        }
    }
    
    public sealed partial class EditorUtils
    {
        public static GUIStyle RichTextStyle;

        public static float LineHeight;

        public static float HorizontalBuffer;
        public static float IndentSpace;

        public static float VerticalBuffer;
        public static float VerticalSpace;

        public static string EditedValue;
        public static string EditedControl;

        private static int previousIndent;
        private static float previousFieldWidth;
        private static float previousLabelWidth;

        private static int lineCount;
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            RichTextStyle = new GUIStyle() { richText = true, alignment = TextAnchor.MiddleLeft, fontSize = 14 };
            LineHeight = EditorGUIUtility.singleLineHeight;
            HorizontalBuffer = 2f;
            IndentSpace = 15f;
            VerticalBuffer = 3f;
            VerticalSpace = 9f;
            EditedValue = string.Empty;
            EditedControl = string.Empty;
            lineCount = 0;
        }

        public static Indent StartIndent(PropertyDrawerRect rect)
        {
            return StartIndent(rect, 1);
        }

        public static Indent StartIndent(PropertyDrawerRect rect, int indentAmount)
        {
            return new Indent(rect, IndentSpace * indentAmount);
        }
        
        public static Rect BeginPropertyDrawer(Rect rect)
        {
            previousIndent = EditorGUI.indentLevel;
            previousLabelWidth = EditorGUIUtility.labelWidth;
            previousFieldWidth = EditorGUIUtility.fieldWidth;
            
            Rect indentedPosition = EditorGUI.IndentedRect(rect);
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;
            
            return indentedPosition;
        }

        public static void EndPropertyDrawer()
        {
            EditorGUI.indentLevel = previousIndent;
            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUIUtility.fieldWidth = previousFieldWidth;
        }

        public static Rect[] SplitLineRect(Rect lineRect, GUIContent[] labelContent, GUIStyle labelStyle, int linesDownward)
        {
            float[] sizePercentages = new float[labelContent.Length];

            for (int i = 0; i < sizePercentages.Length; i++)
            {
                sizePercentages[i] = 1f / sizePercentages.Length;
            }

            return SplitLineRect(lineRect, labelContent, sizePercentages, labelStyle, linesDownward);
        }

        public static Rect[] SplitLineRect(Rect lineRect, GUIContent[] labelContent, float[] sizePercentages, GUIStyle labelStyle, int linesDownward)
        {
            Debug.Assert(labelContent.Length == sizePercentages.Length);
            
            Rect[] lineRects = new Rect[labelContent.Length * 2];
            
            float height = labelStyle.CalcSize(GUIContent.none).y;
            float verticalBuffer = VerticalBuffer;
            float additionalX = 0f;
            float additionalY = (height + verticalBuffer) * linesDownward;

            for (int i = 0; i < labelContent.Length; i++)
            {
                float labelFieldGroupWidth = lineRect.width * sizePercentages[i];

                float horizontalBuffer = i > 0
                    ? HorizontalBuffer
                    : 0f;

                Rect labelRect = new Rect(lineRect.x + additionalX, lineRect.y + additionalY, labelStyle.CalcSize(labelContent[i]).x + horizontalBuffer, height);
                Rect fieldRect = new Rect(lineRect.x + additionalX + labelRect.width, lineRect.y + additionalY, labelFieldGroupWidth - labelRect.width, height);

                lineRects[i * 2] = labelRect;
                lineRects[i * 2 + 1] = fieldRect;

                additionalX += labelFieldGroupWidth;
            }

            return lineRects;
        }

        public enum FieldType { Text, Toggle }
        private const int SMALLEST_ALLOWED_TEXT_FIELD_SIZE = 25;
        private const int SMALLEST_ALLOWED_TOGGLE_FIELD_SIZE = 12;

        //public static Rect[] ReplicateDefaultSplitLineRect(Rect lineRect, GUIContent[] labels, GUIStyle labelStyle)
        //{
        //    return ReplicateDefaultSplitLineRect(lineRect, labels, labels.Select(label => FieldType.Text).Take(Mathf.Max(labels.Length - 1, 1)).ToArray(), labelStyle);
        //}

        public static Rect[] ReplicateDefaultSplitLineRect(Rect lineRect, GUIContent[] labels, FieldType[] fieldTypes, GUIStyle labelStyle)
        {
            Debug.Assert(labels.Length > 1);

            Rect leadingRect = LeadingLabel(lineRect, labels[0]);
            lineRect.xMin += leadingRect.width;

            if(labels.Length == 1)
            {
                return new Rect[] { leadingRect, lineRect };
            }
            else
            {
                GUIContent[] scaledLabels = new GUIContent[labels.Length - 1];

                for(int i = 1; i < labels.Length; i++)
                {
                    scaledLabels[i - 1] = labels[i];
                }

                Rect[] scaledRects = ScaledSplitLineRect(lineRect, scaledLabels, fieldTypes, labelStyle);
                Rect[] allRects = new Rect[scaledRects.Length + 1];
                allRects[0] = leadingRect;

                for(int i = 1; i < allRects.Length; i++)
                {
                    allRects[i] = scaledRects[i - 1];
                }

                return allRects;
            }
        }

        public static Rect[] ScaledSplitLineRect(Rect lineRect, GUIContent[] labels, FieldType[] fieldTypes, GUIStyle labelStyle)
        {
            int fields = fieldTypes.Length;
            int textFields = fieldTypes
                .Where(type => type == FieldType.Text)
                .Count();
            int toggleFields = fieldTypes
                .Where(type => type == FieldType.Toggle)
                .Count();
            int spaces = 2 + Mathf.Max(labels.Length - 1, 0) * 2;

            //min sizes
            float[] labelsWidthActual = labels
                .Select(label => labelStyle.CalcSize(label).x)
                .ToArray();
            float textFieldsWidthMinimum = SMALLEST_ALLOWED_TEXT_FIELD_SIZE * textFields;

            //ideal sizes
            float labelsWidth = labelsWidthActual.Sum();
            float textFieldsWidth = textFields * EditorGUIUtility.fieldWidth;
            float toggleFieldsWidth = SMALLEST_ALLOWED_TOGGLE_FIELD_SIZE * toggleFields;
            float spacingWidth = spaces * HorizontalBuffer;

            //calculations
            float totalWidth = labelsWidth + textFieldsWidth + toggleFieldsWidth + spacingWidth;

            if (totalWidth < lineRect.width)
            {
                float multiplier = (lineRect.width - (spacingWidth + toggleFieldsWidth + labelsWidth)) / (totalWidth - (spacingWidth + toggleFieldsWidth + labelsWidth));

                if (textFieldsWidth > 0)
                {
                    textFieldsWidth *= multiplier;
                }
                else
                {
                    spacingWidth *= multiplier;
                }
            }
            else if (totalWidth > lineRect.width)
            {
                float multiplier = (lineRect.width - (spacingWidth + toggleFieldsWidth + labelsWidth)) / (totalWidth - (spacingWidth + toggleFieldsWidth + labelsWidth));

                textFieldsWidth = Mathf.Max(textFieldsWidth * multiplier, textFieldsWidthMinimum);
            }

            Rect[] rects = new Rect[labels.Length + fields];

            for (int i = 0; i < labels.Length + fields + spaces; i++)
            {
                if (i % 4 == 4 || i % 4 == 0)
                {
                    rects[i - i / 2] = new Rect(lineRect.x, lineRect.y, labelsWidthActual[i / 4], lineRect.height);
                    lineRect.xMin += rects[i - i / 2].width;
                }
                else if (i % 4 == 1 || i % 4 == 3)
                {
                    lineRect.xMin += spacingWidth / spaces;
                }
                else if (i % 4 == 2)
                {
                    float width = fieldTypes[i / 4] == FieldType.Text
                        ? textFieldsWidth / textFields
                        : toggleFieldsWidth / toggleFields;

                    rects[i - i / 2] = new Rect(lineRect.x, lineRect.y, width, lineRect.height);
                    lineRect.xMin += rects[i - i / 2].width;
                }
            }

            return rects;
        }

        public static Rect LeadingLabel(Rect lineRect, GUIContent label)
        {
            Rect labelRect = new Rect(lineRect.x, lineRect.y, EditorGUIUtility.labelWidth, lineRect.height);
            lineRect.xMin += EditorGUIUtility.labelWidth;
            
            return labelRect;
        }

        public static float MinDefaultSpaceNeeded(GUIContent[] labels, GUIStyle labelStyle)
        {
            Debug.Assert(labels.Length > 1);

            //amount of elements
            int additionalLabels = labels.Length - 1;
            int fields = Mathf.Max(additionalLabels, 1);
            int spaces = 1 + Mathf.Max(additionalLabels - 1, 0) * 2;
            
            float[] labelsWidthActual = labels
                .Select(label => labelStyle.CalcSize(label).x)
                .ToArray();
            labelsWidthActual[0] = EditorGUIUtility.labelWidth;

            float fieldsWidthMinimum = SMALLEST_ALLOWED_TEXT_FIELD_SIZE * fields;
            float additionalLabelsWidth = labelsWidthActual.Sum();
            float spacingWidth = spaces * HorizontalBuffer;

            //calculations
            float totalWidth = fieldsWidthMinimum + additionalLabelsWidth + spacingWidth;

            return totalWidth;
        }

        public static void StartCountingLines()
        {
            lineCount = 0;
        }

        public static int GetLineCount()
        {
            return lineCount;
        }

        public static Rect GetRectBelow(Rect fullRect, GUIStyle labelStyle, float linesHigh)
        {
            return new Rect(fullRect.x, fullRect.yMax + VerticalBuffer, fullRect.width, labelStyle.CalcSize(GUIContent.none).y * linesHigh);
        }

        public static Rect GetRectBelow(Rect fullRect, GUIStyle labelStyle)
        {
            return new Rect(fullRect.x, fullRect.yMax + VerticalBuffer, fullRect.width, labelStyle.CalcSize(GUIContent.none).y);
        }

        public static Rect GetRectBelow(Rect fullRect, float height, float linesHigh)
        {
            return new Rect(fullRect.x, fullRect.yMax + VerticalBuffer, fullRect.width, height * linesHigh);
        }

        public static Rect GetRectBelow(Rect fullRect, float height)
        {
            return new Rect(fullRect.x, fullRect.yMax + VerticalBuffer, fullRect.width, height);
        }

        public static Rect GetTopRect(Rect fullRect, GUIStyle labelStyle, float linesHigh)
        {
            return new Rect(fullRect.x, fullRect.y, fullRect.width, labelStyle.CalcSize(GUIContent.none).y * linesHigh);
        }

        public static Rect GetTopRect(Rect fullRect, GUIStyle labelStyle)
        {
            return new Rect(fullRect.x, fullRect.y, fullRect.width, labelStyle.CalcSize(GUIContent.none).y);
        }

        public static Rect GetTopRect(Rect fullRect, float height, float linesHigh)
        {
            return new Rect(fullRect.x, fullRect.y, fullRect.width, height * linesHigh);
        }

        public static Rect GetTopRect(Rect fullRect, float height)
        {
            return new Rect(fullRect.x, fullRect.y, fullRect.width, height);
        }
        
        public enum TextOptions
        {
            Bold,
            Italic
        }

        public static string GetRichText(string text, string color, int size, params TextOptions[] options)
        {
            options = options == null
                ? new TextOptions[0]
                : options;

            string richText = string.Format("<size={0}><color={1}>{2}</color></size>", size, color, text);

            richText = options.Contains(TextOptions.Bold)
                ? string.Concat("<b>", richText, "</b>")
                : richText;

            richText = options.Contains(TextOptions.Italic)
                ? string.Concat("<i>", richText, "</i>")
                : richText;

            return richText;
        }

        public static string GetRichText(string text, string color, params TextOptions[] options)
        {
            return GetRichText(text, color, 11, options);
        }

        public static string GetRichText(string text, int size, params TextOptions[] options)
        {
            return GetRichText(text, "black", size, options);
        }

        public static string GetRichText(string text, params TextOptions[] options)
        {
            return GetRichText(text, "black", 11, options);
        }
        
        public static float[] FloatArrayField(GUIContent arrayLengthContent, GUIContent arrayElementContent, float[] array)
        {
            int length = EditorUtils.ArraySizeField(arrayLengthContent, array.Length);

            float[] temp = new float[length];
            CopyFromTo(array, temp);

            array = new float[length];
            CopyFromTo(temp, array);

            EditorGUI.indentLevel++;

            for (int i = 0; i < length; i++)
            {
                var currentContent = new GUIContent(string.Format("{0} {1}", arrayElementContent.text, i + 1));
                        
                array[i] = EditorGUILayout.FloatField(currentContent, array[i]);
            }

            EditorGUI.indentLevel--;

            return array;
        }

        public static float[] FloatArrayField(Rect rect, GUIContent arrayLengthContent, GUIContent arrayElementContent, float[] array)
        {
            Rect top = EditorUtils.GetTopRect(rect, EditorStyles.numberField);

            int length = EditorUtils.ArraySizeField(top, arrayLengthContent, array.Length);
            
            float[] temp = new float[length];
            CopyFromTo(array, temp);

            array = new float[length];
            CopyFromTo(temp, array);
            
            Rect next = GetRectBelow(top, EditorStyles.numberField, 1);

            EditorGUI.indentLevel++;

            for (int i = 0; i < length; i++)
            {
                var currentContent = new GUIContent(string.Format("{0} {1}", arrayElementContent.text, i + 1));

                array[i] = EditorGUI.FloatField(next, currentContent, array[i]);

                next = GetRectBelow(next, EditorStyles.numberField, 1);
            }

            EditorGUI.indentLevel--;

            return array;
        }

        /// <summary>
        /// Creates an special IntField that only changes the actual value when pressing enter or losing focus
        /// </summary>
        /// <param name="label">The label of the int field</param>
        /// <param name="value">The value of the intfield</param>
        /// <returns>The valuefo the intfield</returns>
        public static int ArraySizeField(GUIContent label, int value, bool multipleDifferentValues = false)
        {
            float editing = EditorGUIUtility.hotControl;

            // Get current control id
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            
            // Assign real value if out of focus or enter pressed,
            // the edited value cannot be empty and the tooltip must match to the current control

            if ((EditedValue != string.Empty && EditedControl == controlID.ToString()) &&
                ((Event.current.Equals(Event.KeyboardEvent("[enter]")) || Event.current.Equals(Event.KeyboardEvent("return")) ||
                Event.current.Equals(Event.KeyboardEvent("tab")) || (Event.current.type == EventType.MouseDown))))
            {

                // Draw textfield, somehow this makes it work better when pressing enter
                // No idea why...
                EditedValue = EditorGUILayout.TextField(label, EditedValue, EditorStyles.numberField);

                // Parse number
                int number = 0;

                if (int.TryParse(EditedValue, out number))
                {
                    value = Mathf.Min(number, 100);
                }

                // Reset values, the edit value must go back to its original state
                EditedValue = string.Empty;
                EditedControl = string.Empty;
                GUI.changed = true;

                return value;
            }
            else if (EditedControl != controlID.ToString())
            {
                EditorGUILayout.BeginHorizontal();
                GUI.SetNextControlName(controlID.ToString());
                string entered = multipleDifferentValues
                    ? EditorGUILayout.TextField(label, "-", EditorStyles.numberField)
                    : EditorGUILayout.TextField(label, value.ToString(), EditorStyles.numberField);
                EditorGUILayout.EndHorizontal();

                if (GUI.GetNameOfFocusedControl() == controlID.ToString())
                {
                    EditedControl = controlID.ToString();
                    EditedValue = entered;
                }
            }
            else
            {
                GUI.SetNextControlName(controlID.ToString());

                string entered = multipleDifferentValues
                    ? EditorGUILayout.TextField(label, "-", EditorStyles.numberField)
                    : EditorGUILayout.TextField(label, EditedValue, EditorStyles.numberField);

                EditedValue = entered != "-"
                        ? entered
                        : value.ToString();
            }

            GUI.changed = false;

            return value;
        }

        public static void ArrayDragAndDropArea<Type>(Rect controlRect, SerializedProperty array)
            where Type : UnityEngine.Object
        {
            Type[] droppedObjects = DetectDraggedObjects<Type>(controlRect);

            if(droppedObjects.Length > 0)
            {
                array.serializedObject.ApplyModifiedProperties();

                for (int j = 0; j < array.serializedObject.targetObjects.Length; j++)
                {
                    UnityEngine.Object currentTarget = array.serializedObject.targetObjects[j];

                    Type[] currentArray = ReflectionUtils.GetMemberAtPath<Type[]>(currentTarget, array.propertyPath);
                    Array.Resize(ref currentArray, currentArray.Length + droppedObjects.Length);
                    
                    for (int i = currentArray.Length - droppedObjects.Length; i < currentArray.Length; i++)
                    {
                        currentArray[i] = droppedObjects[i - (currentArray.Length - droppedObjects.Length)];
                    }

                    ReflectionUtils.SetMemberAtPath(currentTarget, currentArray, array.propertyPath);
                }

                array.serializedObject.Update();

                GUI.changed = true;
            }
        }

        public static Type[] DetectDraggedObjects<Type>(Rect controlRect)
            where Type : UnityEngine.Object
        {
            Type[] convertedObjects = new Type[0];

            if (controlRect.Contains(Event.current.mousePosition)
                && (Event.current.type == EventType.DragUpdated
                || Event.current.type == EventType.DragExited))
            {
                convertedObjects = DragAndDrop.objectReferences
                    .Select(obj =>
                    {
                        if (typeof(Type).IsAssignableFrom(obj.GetType()))
                        {
                            return (Type)obj;
                        }
                        else if (typeof(GameObject).IsAssignableFrom(obj.GetType()))
                        {
                            GameObject gameObject = (GameObject)obj;
                            Type component = gameObject.GetComponent<Type>();

                            if (component != null)
                            {
                                return component;
                            }
                        }

                        return null;
                    })
                    .Where(obj => obj != null)
                    .ToArray();
            }
            
            if (controlRect.Contains(Event.current.mousePosition)
                && Event.current.type == EventType.DragUpdated
                && convertedObjects.Any())
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
            
            if (controlRect.Contains(Event.current.mousePosition)
                && Event.current.type == EventType.DragExited
                && convertedObjects.Any())
            {
                DragAndDrop.AcceptDrag();

                return convertedObjects;
            }

            return new Type[0];
        }

        /// <summary>
        /// Creates an special IntField that only changes the actual value when pressing enter or losing focus
        /// </summary>
        /// <param name="label">The label of the int field</param>
        /// <param name="value">The value of the intfield</param>
        /// <returns>The valuefo the intfield</returns>
        public static int ArraySizeField(Rect rect, GUIContent label, int value, bool multipleDifferentValues = false)
        {
            // Get current control id
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            
            // Assign real value if out of focus or enter pressed,
            // the edited value cannot be empty and the tooltip must match to the current control
            
            if ((EditedValue != string.Empty && EditedControl == controlID.ToString()) &&
                ((Event.current.Equals(Event.KeyboardEvent("[enter]")) || Event.current.Equals(Event.KeyboardEvent("return")) ||
                Event.current.Equals(Event.KeyboardEvent("tab")) || (Event.current.type == EventType.MouseDown))))
            {

                // Draw textfield, somehow this makes it work better when pressing enter
                // No idea why...
                EditedValue = EditorGUI.TextField(rect, label, EditedValue, EditorStyles.numberField);

                // Parse number
                int number = 0;
                if (int.TryParse(EditedValue, out number))
                {
                    value = Mathf.Min(number, 100);
                }

                // Reset values, the edit value must go back to its original state
                EditedValue = string.Empty;
                EditedControl = string.Empty;
                GUI.changed = true;
                
                return value;
            }
            else if (EditedControl != controlID.ToString())
            {
                GUI.SetNextControlName(controlID.ToString());

                string entered = multipleDifferentValues
                    ? EditorGUI.TextField(rect, label, "-", EditorStyles.numberField)
                    : EditorGUI.TextField(rect, label, value.ToString(), EditorStyles.numberField);
                
                if (GUI.GetNameOfFocusedControl() == controlID.ToString())
                {
                    EditedControl = controlID.ToString();
                    EditedValue = entered != "-"
                        ? entered
                        : value.ToString();
                }
            }
            else
            {
                GUI.SetNextControlName(controlID.ToString());

                string entered = multipleDifferentValues
                    ? EditorGUI.TextField(rect, label, "-", EditorStyles.numberField)
                    : EditorGUI.TextField(rect, label, EditedValue, EditorStyles.numberField);

                EditedValue = entered != "-"
                        ? entered
                        : value.ToString();
            }

            GUI.changed = false;

            return value;
        }

        public static void ArraySizeField(Rect rect, SerializedProperty array, GUIContent label)
        {
            bool guiChanged = false;

            // Get current control id
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            // Assign real value if out of focus or enter pressed,
            // the edited value cannot be empty and the tooltip must match to the current control

            if ((EditedValue != string.Empty && EditedControl == controlID.ToString()) &&
                ((Event.current.Equals(Event.KeyboardEvent("[enter]")) || Event.current.Equals(Event.KeyboardEvent("return")) ||
                Event.current.Equals(Event.KeyboardEvent("tab")) || (Event.current.type == EventType.MouseDown))))
            {

                // Draw textfield, somehow this makes it work better when pressing enter
                // No idea why...
                EditedValue = EditorGUI.TextField(rect, label, EditedValue, EditorStyles.numberField);

                // Parse number
                int number = 0;
                if (int.TryParse(EditedValue, out number))
                {
                    array.arraySize = Mathf.Min(number, 100);
                }

                // Reset values, the edit value must go back to its original state
                EditedValue = string.Empty;
                EditedControl = string.Empty;
                guiChanged = true;

                return;
            }
            else if (EditedControl != controlID.ToString())
            {
                GUI.SetNextControlName(controlID.ToString());

                string entered = array.hasMultipleDifferentValues
                    ? EditorGUI.TextField(rect, label, "-", EditorStyles.numberField)
                    : EditorGUI.TextField(rect, label, array.arraySize.ToString(), EditorStyles.numberField);

                if (GUI.GetNameOfFocusedControl() == controlID.ToString())
                {
                    EditedControl = controlID.ToString();
                    EditedValue = entered != "-"
                        ? entered
                        : array.arraySize.ToString();
                }
            }
            else
            {
                GUI.SetNextControlName(controlID.ToString());

                string entered = array.hasMultipleDifferentValues
                    ? EditorGUI.TextField(rect, label, "-", EditorStyles.numberField)
                    : EditorGUI.TextField(rect, label, EditedValue, EditorStyles.numberField);

                EditedValue = entered != "-"
                        ? entered
                        : array.arraySize.ToString();
            }

            GUI.changed = guiChanged;
        }

        public static void ArraySizeField<Type>(Rect rect, SerializedProperty array, GUIContent label)
            where Type : UnityEngine.Object
        {
            ArraySizeField(rect, array, label);

            Rect dragAndDropArea = new Rect(rect.x, rect.y, EditorStyles.numberField.CalcSize(label).x, rect.height);

            EditorGUI.BeginChangeCheck();

            ArrayDragAndDropArea<Type>(dragAndDropArea, array);

            if(EditorGUI.EndChangeCheck())
            {
                EditedValue = $"{array.arraySize}";
            }
        }

        public static void DisplayMultilinePropertyArray(SerializedProperty arrayProperty, GUIContent elementContent, bool highlight = false, bool addSpacing = false, int? length = null)
        {
            if (arrayProperty.isArray)
            {
                length = length.HasValue
                    ? length.Value
                    : arrayProperty.arraySize;

                for (int i = 0; i < length; i++)
                {
                    SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);

                    EditorGUILayout.LabelField(new GUIContent(GetRichText(string.Format("{0} {1}", elementContent.text, i + 1), TextOptions.Bold), elementContent.tooltip), RichTextStyle);

                    if(addSpacing)
                    {
                        EditorGUILayout.Space();
                    }

                    Rect elementRect = EditorGUILayout.GetControlRect(false, EditorGUI.GetPropertyHeight(element));
                    
                    EditorGUI.BeginChangeCheck();

                    if(highlight)
                    {
                        GUI.Box(elementRect, GUIContent.none);
                    }

                    EditorGUI.PropertyField(elementRect, element);

                    if (EditorGUI.EndChangeCheck())
                    {
                        GUI.changed = true;
                    }

                    EditorGUILayout.Space();
                }
            }
        }

        public static void DisplaySinglelinePropertyArray(SerializedProperty arrayProperty, GUIContent elementContent)
        {
            if (arrayProperty.isArray)
            {
                for (int i = 0; i < arrayProperty.arraySize; i++)
                {
                    SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);
                    
                    GUIContent currentContent = new GUIContent(string.Format("{0} {1}:", elementContent.text, i + 1), elementContent.tooltip);

                    Rect elementRect = EditorGUILayout.GetControlRect(true, LineHeight);

                    EditorGUI.BeginChangeCheck();

                    EditorGUI.PropertyField(elementRect, element, currentContent);

                    if (EditorGUI.EndChangeCheck())
                    {
                        GUI.changed = true;
                    }

                    EditorGUILayout.Space();
                }
            }
        }

        public static void DisplaySinglelinePropertyArray(Rect rect, SerializedProperty arrayProperty, GUIContent elementContent)
        {
            if (arrayProperty.isArray)
            {
                Rect currentRect = GetTopRect(rect, LineHeight);

                for (int i = 0; i < arrayProperty.arraySize; i++)
                {
                    SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);

                    GUIContent currentContent = new GUIContent(string.Format("{0} {1}:", elementContent.text, i + 1), elementContent.tooltip);

                    EditorGUI.BeginChangeCheck();

                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(currentRect, element, currentContent);
                    EditorGUI.indentLevel--;

                    if (EditorGUI.EndChangeCheck())
                    {
                        GUI.changed = true;
                    }

                    currentRect = GetRectBelow(currentRect, LineHeight);
                }
            }
        }

        public static void DisplaySinglelinePropertyArray(Rect rect, SerializedProperty objectArray, GUIContent elementContent, Type type)
        {
            if (objectArray.isArray)
            {
                Rect currentRect = GetTopRect(rect, LineHeight);

                for (int i = 0; i < objectArray.arraySize; i++)
                {
                    SerializedProperty element = objectArray.GetArrayElementAtIndex(i);

                    GUIContent currentContent = new GUIContent(string.Format("{0} {1}:", elementContent.text, i + 1), elementContent.tooltip);

                    EditorGUI.BeginChangeCheck();

                    EditorGUI.ObjectField(currentRect, element, type, elementContent);

                    if (EditorGUI.EndChangeCheck())
                    {
                        GUI.changed = true;
                    }

                    currentRect = GetRectBelow(currentRect, LineHeight);
                }
            }
        }

        public static float GetMultilinePropertyArrayHeight(SerializedProperty arrayProperty, GUIContent elementContent)
        {
            float elementLabelHeight = EditorStyles.boldLabel.CalcSize(elementContent).y;
            float totalHeight = 0f;

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);

                totalHeight += elementLabelHeight;
                totalHeight += EditorGUI.GetPropertyHeight(element);
            }

            totalHeight += (arrayProperty.arraySize * 2 - 1) * VerticalBuffer;

            return totalHeight;
        }

        public static float GetSinglelinePropertyArrayHeight(SerializedProperty arrayProperty, GUIContent elementContent)
        {
            return LineHeight * arrayProperty.arraySize + VerticalBuffer * (arrayProperty.arraySize - 1);
        }
        
        public static ValueGenerationMode GetValueGenerationMode(SerializedProperty curveGroup, SerializedProperty listGroup)
        {
            SerializedProperty useList = listGroup.FindPropertyRelative("useControl");
            SerializedProperty useCurve = curveGroup.FindPropertyRelative("useControl");
            SerializedProperty curveMode = curveGroup.FindPropertyRelative("minMaxCurve.m_Mode");

            Debug.Assert(!(useCurve.boolValue && useList.boolValue));
            
            if (useCurve.boolValue)
            {
                switch ((ParticleSystemCurveMode)curveMode.intValue)
                {
                    case ParticleSystemCurveMode.Constant:
                        return ValueGenerationMode.Constant;
                    case ParticleSystemCurveMode.TwoConstants:
                        return ValueGenerationMode.BetweenValues;
                    case ParticleSystemCurveMode.Curve:
                        return ValueGenerationMode.Curve;
                    case ParticleSystemCurveMode.TwoCurves:
                        return ValueGenerationMode.BetweenCurves;
                    default:
                        throw new Exception("Unity added new particle system stuff and this must not be working now... This is not compatible with your version of Unity.");
                }
            }
            else if (useList.boolValue)
            {
                return ValueGenerationMode.List;
            }
            else
            {
                return ValueGenerationMode.None;
            }
        }

        public static void SetValueGenerationMode(SerializedProperty curveGroup, SerializedProperty listGroup, ValueGenerationMode valueGenerationMode)
        {
            SerializedProperty useList = listGroup.FindPropertyRelative("useControl");
            SerializedProperty useCurve = curveGroup.FindPropertyRelative("useControl");
            SerializedProperty curveMode = curveGroup.FindPropertyRelative("minMaxCurve.m_Mode");

            Debug.Assert(!(useCurve.boolValue && useList.boolValue));

            switch (valueGenerationMode)
            {
                case ValueGenerationMode.Constant:
                    useList.boolValue = false;
                    useCurve.boolValue = true;
                    curveMode.intValue = (int)ParticleSystemCurveMode.Constant;
                    break;
                case ValueGenerationMode.BetweenValues:
                    useList.boolValue = false;
                    useCurve.boolValue = true;
                    curveMode.intValue = (int)ParticleSystemCurveMode.TwoConstants;
                    break;
                case ValueGenerationMode.Curve:
                    useList.boolValue = false;
                    useCurve.boolValue = true;
                    curveMode.intValue = (int)ParticleSystemCurveMode.Curve;
                    break;
                case ValueGenerationMode.BetweenCurves:
                    useList.boolValue = false;
                    useCurve.boolValue = true;
                    curveMode.intValue = (int)ParticleSystemCurveMode.TwoCurves;
                    break;
                case ValueGenerationMode.List:
                    useList.boolValue = true;
                    useCurve.boolValue = false;
                    break;
                case ValueGenerationMode.None:
                    useList.boolValue = false;
                    useCurve.boolValue = false;
                    break;
                default:
                    Debug.LogWarning("Intensity modes were added but asserts were not... Potentially dangerous.");
                    break;
            }
        }

        private const float minIntensityModifierFieldWidth = 50f;
        private static GUIContent intensityModifiersScaleContent = new GUIContent("Scale: ");
        private static GUIContent intensityModifiersOffsetContent = new GUIContent("Offset: ");
        private static GUIContent intensityModifiersRelateInverselyContent = new GUIContent("Relate Inversely: ");
        private static GUIContent[] intensityModifiersElementContent = new GUIContent[]
        {
                    new GUIContent("Light Intensity:", ""),
                    new GUIContent("Light Color Intensity:", ""),
                    new GUIContent("Light Material Intensity:", ""),
                    new GUIContent("Light Range:", ""),
                    new GUIContent("Spotlight Angle:", "")
        };

        public static void DisplayIntensityModifiers(Rect rect, SerializedProperty controlTargetProperty, SerializedProperty intensityModifiersProperty)
        {
            string currentBinaryString = Convert.ToString(controlTargetProperty.intValue, 2);
            
            Debug.Assert(currentBinaryString.Count() <= intensityModifiersProperty.arraySize || currentBinaryString.Count() == Convert.ToString(~0, 2).Count());

            if(currentBinaryString.Count(character => character == '1') > 1)
            {
                Rect currentRect = GetTopRect(rect, EditorStyles.label);

                for (int i = 0; i < currentBinaryString.Length && i < intensityModifiersProperty.arraySize; i++) // the only time the second condition will stop the loop is when currentProperty.ControlTargetProperty is set to IntensityControlTarget.Everything
                {
                    if (currentBinaryString[currentBinaryString.Length - 1 - i] == '1') //The binary number will have the lowest values on the right and not the left
                    {
                        SerializedProperty element = intensityModifiersProperty.GetArrayElementAtIndex(i);
                        
                        GUIContent contentAtIndex = intensityModifiersElementContent[i];

                        EditorGUI.PropertyField(currentRect, element, contentAtIndex);
                        
                        currentRect = GetRectBelow(currentRect, EditorStyles.label);
                    }
                }
            }
        }
        
        public static float GetIntensityModifiersHeight(SerializedProperty controlTargetProperty)
        {
            int lines = Mathf.Min(Convert.ToString(controlTargetProperty.intValue, 2).Count(character => character == '1'), 5);

            lines = lines > 1
                ? lines
                : 0;

            return LineHeight * lines + VerticalBuffer * (lines - 1);
        }

        private static readonly GUIContent intensityControlTargetLabelContent = new GUIContent("Affecting Properties");

        public static void DisplayIntensityControlTargetField(Rect rect, SerializedProperty intensityControlTargetProperty)
        {
            IntensityControlTarget previous = (IntensityControlTarget)intensityControlTargetProperty.intValue;

            EditorGUI.BeginChangeCheck();
            intensityControlTargetProperty.intValue = (int)(IntensityControlTarget)EditorGUI.EnumFlagsField(rect, intensityControlTargetLabelContent, (IntensityControlTarget)intensityControlTargetProperty.intValue);

            if(EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                intensityControlTargetProperty.serializedObject.ApplyModifiedProperties();

                IntensityControlTarget current = (IntensityControlTarget)intensityControlTargetProperty.intValue;

                for (int j = 0; j < intensityControlTargetProperty.serializedObject.targetObjects.Length; j++)
                {
                    LightControlOption option = (LightControlOption)intensityControlTargetProperty.serializedObject.targetObjects[j];

                    GameObject.FindObjectsOfType<GroupedLightController>()
                        .SelectMany(groupedController => ReflectionUtils.GetMemberAtPath<LightControllerGroup[]>(groupedController, "lightControllerGroups"))
                        .Where(group => ReflectionUtils.GetMemberAtPath<InstancedControlOption[]>(group, "controlOptions")
                            .Any(controlOption => group != null && InstancedVersionOf(controlOption, option))) //controlOption.InstancedVersionOf(option)))
                        .ToList().ForEach(controllerGroup =>
                        {
                            ControlOptionGroup optionGroup = ReflectionUtils.GetMemberAtPath<ControlOptionGroup>(controllerGroup, "controlOptionGroup");

                            if (optionGroup.SaveLightColor
                            && previous.HasFlag(IntensityControlTarget.LightColorIntensity)
                            && !current.HasFlag(IntensityControlTarget.LightColorIntensity))
                            {
                                Debug.Assert(optionGroup.Lights.Length == optionGroup.LightColors.Length);

                                for (int i = 0; i < optionGroup.Lights.Length; i++)
                                {
                                    if (optionGroup.Lights[i] != null)
                                    {
                                        optionGroup.Lights[i].color = optionGroup.LightColors[i];
                                    }
                                }
                            }

                            if (optionGroup.SaveMaterialColor
                            && previous.HasFlag(IntensityControlTarget.MaterialColorIntensity)
                            && !current.HasFlag(IntensityControlTarget.MaterialColorIntensity))
                            {
                                Debug.Assert(optionGroup.EmissiveMaterialRenderers.Length == optionGroup.MaterialColors.Length);

                                for (int i = 0; i < optionGroup.EmissiveMaterialRenderers.Length; i++)
                                {
                                    if (optionGroup.EmissiveMaterialRenderers[i] != null)
                                    {
                                        for (int k = 0; k < optionGroup.EmissiveMaterialRenderers[i].materials.Length; k++)
                                        {
                                            if (optionGroup.EmissiveMaterialRenderers[i].materials[k] != null)
                                            {
                                                optionGroup.EmissiveMaterialRenderers[i].materials[k].SetColor("_EmissionColor", optionGroup.MaterialColors[i][k]);
                                            }
                                        }
                                    }
                                }
                            }
                        });
                }

                intensityControlTargetProperty.serializedObject.Update();
            }
        }


        public static void DisplayColorControlTargetField(Rect rect, SerializedProperty colorControlTargetProperty)
        {
            ColorControlTarget previous = (ColorControlTarget)colorControlTargetProperty.intValue;

            EditorGUI.BeginChangeCheck();
            colorControlTargetProperty.intValue = (int)(ColorControlTarget)EditorGUI.EnumFlagsField(rect, intensityControlTargetLabelContent, (ColorControlTarget)colorControlTargetProperty.intValue);

            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                colorControlTargetProperty.serializedObject.ApplyModifiedProperties();

                ColorControlTarget current = (ColorControlTarget)colorControlTargetProperty.intValue;

                for (int i = 0; i < colorControlTargetProperty.serializedObject.targetObjects.Length; i++)
                {
                    LightControlOption option = (LightControlOption)colorControlTargetProperty.serializedObject.targetObjects[i];

                    GameObject.FindObjectsOfType<GroupedLightController>()
                        .SelectMany(groupedController => ReflectionUtils.GetMemberAtPath<LightControllerGroup[]>(groupedController, "lightControllerGroups"))
                        .Where(group => ReflectionUtils.GetMemberAtPath<InstancedControlOption[]>(group, "controlOptions")
                            .Any(controlOption => group != null && InstancedVersionOf(controlOption, option))) //controlOption.InstancedVersionOf(option)))
                        .ToList().ForEach(controllerGroup =>
                        {
                            ControlOptionGroup optionGroup = ReflectionUtils.GetMemberAtPath<ControlOptionGroup>(controllerGroup, "controlOptionGroup");
                            LightControllerGroupData controllerGroupData = ReflectionUtils.GetMemberAtPath<LightControllerGroupData>(controllerGroup, "controllerGroupData");

                            if ((current ^ previous).HasFlag(ColorControlTarget.Light))
                            {
                                bool hasLightColorController = controllerGroupData.LightControlOptions
                                    .Any(controlOption => controlOption is ColorControlOption &&
                                                         (controlOption as ColorControlOption).ColorTarget.HasFlag(ColorControlTarget.Light));

                                ReflectionUtils.SetMemberAtPath(optionGroup, !hasLightColorController, "saveLightColor");
                            }

                            if ((current ^ previous).HasFlag(ColorControlTarget.Material))
                            {
                                bool hasMaterialColorController = controllerGroupData.LightControlOptions
                                    .Any(controlOption => controlOption is ColorControlOption &&
                                                         (controlOption as ColorControlOption).ColorTarget.HasFlag(ColorControlTarget.Material));

                                ReflectionUtils.SetMemberAtPath(optionGroup, !hasMaterialColorController, "saveMaterialColor");
                            }
                        });
                }

                colorControlTargetProperty.serializedObject.Update();
            }
        }

        public static bool InstancedVersionOf(InstancedControlOption instanced, LightControlOption option)
        {
            if(typeof(IntensityControlOption).IsAssignableFrom(option.GetType()) && instanced.GetType().GetField("intensityControlOption", BindingFlags.Instance | BindingFlags.NonPublic) != null)
            {
                LightControlOption found = ReflectionUtils.GetMemberAtPath<IntensityControlOption>(instanced, "intensityControlOption");

                return found != null && found == option;
            }
            else if (typeof(ColorControlOption).IsAssignableFrom(option.GetType()) && instanced.GetType().GetField("colorControlOption", BindingFlags.Instance | BindingFlags.NonPublic) != null)
            {
                LightControlOption found = ReflectionUtils.GetMemberAtPath<ColorControlOption>(instanced, "colorControlOption");

                return found != null && found == option;
            }
            else if (typeof(AudioControlOption).IsAssignableFrom(option.GetType()) && instanced.GetType().GetField("audioControlOption", BindingFlags.Instance | BindingFlags.NonPublic) != null)
            {
                LightControlOption found = ReflectionUtils.GetMemberAtPath<AudioControlOption>(instanced, "audioControlOption");

                return found != null && found == option;
            }
            else if (typeof(StagedControlOption).IsAssignableFrom(option.GetType()) && instanced.GetType().GetField("stagedControlOption", BindingFlags.Instance | BindingFlags.NonPublic) != null)
            {
                LightControlOption found = ReflectionUtils.GetMemberAtPath<StagedControlOption>(instanced, "stagedControlOption");

                return found != null && found == option;
            }

            return false;
        }

        public class FoundControl
        {
            public GroupedLightController Controller;
            public LightControlOption ControlOption;
            public InstancedControlOption InstancedOption;
            public string OptionPath;
            public string InstancedPath;
        }

        //Only use when the serializedObject's target objects are LightControlOptions
        public static FoundControl[] FindControls<TOption>(SerializedProperty targetIsControlOption)
            where TOption : LightControlOption
        {
            return targetIsControlOption.serializedObject.targetObjects
                .Select(target => (TOption)target)
                .SelectMany(target => GameObject.FindObjectsOfType<GroupedLightController>()
                    .Select(controller => FindIn(controller, target))
                    .Where(found => found != null))
                .ToArray();
        }

        public static FoundControl[] FindControls(LightControlOption controlOption)
        {
            return GameObject.FindObjectsOfType<GroupedLightController>()
                .Select(controller => FindIn(controller, controlOption))
                .Where(found => found != null)
                .ToArray();
        }

        private const string lightControllerGroupDataPath = "lightControllerGroupsData";
        private const string groupLightControlsPath = "lightControlOptions";
        private const string instancedGroupPath = "lightControllerGroups";
        private const string instancedControlOptionsPath = "controlOptions";

        public static FoundControl FindIn(GroupedLightController controller, LightControlOption option)
        {
            LightControllerGroupData[] groupData = ReflectionUtils.GetMemberAtPath<LightControllerGroupData[]>(controller, lightControllerGroupDataPath);

            for(int i = 0; i < groupData.Length; i++)
            {
                LightControlOption[] options = ReflectionUtils.GetMemberAtPath<LightControlOption[]>(groupData[i], groupLightControlsPath);

                for(int k = 0; k < options.Length; k++)
                {
                    if (options[k] == option)
                    {
                        return new FoundControl()
                        {
                            Controller = controller,
                            ControlOption = option,
                            InstancedOption = ReflectionUtils.GetMemberAtPath<InstancedControlOption>(controller, $"{instancedGroupPath}.Array.data[{i}].{instancedControlOptionsPath}.Array.data[{k}]"),
                            OptionPath = $"{lightControllerGroupDataPath}.Array.data[{i}].{groupLightControlsPath}.Array.data[{k}]",
                            InstancedPath = $"{instancedGroupPath}.Array.data[{i}].{instancedControlOptionsPath}.Array.data[{k}]"
                        };
                    }
                    else if (options[k] is StagedControlOption)
                    {
                        FoundControl found = FindIn(controller, (StagedControlOption)options[k], option, $"{lightControllerGroupDataPath}.Array.data[{i}].{groupLightControlsPath}.Array.data[{k}]");

                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
            }

            return null;
        }

        private const string stagedStagesPath = "stager.stages";
        private const string stagedStageControlsPath = "controlOptions";

        private const string instancedStagesPath = "instancedOptionStager.instancedStages";
        private const string instancedStageControlsPath = "controlOptions";

        public static FoundControl FindIn(GroupedLightController controller, StagedControlOption staged, LightControlOption toFind, string path)
        {
            ControlOptionStage[] stages = ReflectionUtils.GetMemberAtPath<ControlOptionStage[]>(staged, stagedStagesPath);

            for(int i = 0; i < stages.Length; i++)
            {
                LightControlOption[] foundControls = ReflectionUtils.GetMemberAtPath<LightControlOption[]>(stages[i], stagedStageControlsPath);

                for (int k = 0; k < foundControls.Length; k++)
                {
                    path += $".{stagedStagesPath}.Array.data[{i}].{stagedStageControlsPath}.Array.data[{k}]";
                    
                    if (foundControls[k] == toFind)
                    {
                        string instancedPath = path
                            .Replace(lightControllerGroupDataPath, instancedGroupPath)
                            .Replace(groupLightControlsPath, instancedControlOptionsPath)
                            .Replace(stagedStagesPath, instancedStagesPath)
                            .Replace(stagedStageControlsPath, instancedStageControlsPath);

                        return new FoundControl()
                        {
                            Controller = controller,
                            ControlOption = toFind,
                            InstancedOption = ReflectionUtils.GetMemberAtPath<InstancedControlOption>(controller, instancedPath),
                            OptionPath = path,
                            InstancedPath = instancedPath
                        };
                    }
                    else if (foundControls[k] is StagedControlOption)
                    {
                        FoundControl found = FindIn(controller, (StagedControlOption)foundControls[k], toFind, path);

                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
            }

            return null;
        }

        public static void UpdateInstancedArray<TSerialized, TInstanced>(
            object[] serializedContainers,
            object[] instancedContainers,
            string serializedPath,
            string instancedPath,
            string iterationsPath,
            Func<TSerialized, TInstanced, bool> isInstancedVersion,
            Func<TSerialized, TInstanced> getInstancedVersion)
            where TSerialized : class
            where TInstanced : class
        {
            for(int i = 0; i < instancedContainers.Length; i++)
            {
                UpdateInstancedArray(
                    serializedContainer: serializedContainers[i],
                    instancedContainer: instancedContainers[i],
                    serializedPath: serializedPath,
                    instancedPath: instancedPath,
                    iterationsPath: iterationsPath,
                    isInstancedVersion: isInstancedVersion,
                    getInstancedVersion: getInstancedVersion);
            }
        }

        public static void UpdateInstancedArray<TSerialized, TInstanced>(
            object serializedContainer,
            object instancedContainer,
            string serializedPath,
            string instancedPath,
            string iterationsPath,
            Func<TSerialized, TInstanced, bool> isInstancedVersion,
            Func<TSerialized, TInstanced> getInstancedVersion)
            where TSerialized : class
            where TInstanced : class
        {
            TSerialized[] savedData = ReflectionUtils.GetMemberAtPath<TSerialized[]>(serializedContainer, serializedPath);
            TInstanced[] instancedData = ReflectionUtils.GetMemberAtPath<TInstanced[]>(instancedContainer, instancedPath);
            int[] iterations = ReflectionUtils.GetMemberAtPath<int[]>(instancedContainer, iterationsPath);
            
            if (savedData.Length == instancedData.Length)
            {
                for (int i = 0; i < savedData.Length; i++)
                {
                    if(!isInstancedVersion(savedData[i], instancedData[i]))
                    {
                        instancedData[i] = getInstancedVersion(savedData[i]);
                    }
                }
            }
            else
            {
                bool addedInfo = savedData.Length > instancedData.Length;
                int previousLength = instancedData.Length;
                int iterationsMin = iterations.Min();

                Array.Resize(ref instancedData, savedData.Length);
                Array.Resize(ref iterations, savedData.Length);

                if (addedInfo)
                {
                    for(int i = previousLength; i < instancedData.Length; i++)
                    {
                        instancedData[i] = savedData[i] != null
                            ? getInstancedVersion(savedData[i])
                            : null;

                        iterations[i] = iterationsMin;
                    }
                }
            } //potentially add more complicated actions for removal--that will only be necissarry if removal is not always from the back though--which currently--I believe it always is

            ReflectionUtils.SetMemberAtPath(instancedContainer, instancedData, instancedPath);
        }

        public static void CopyFromTo<T>(T[] from, T[] to)
        {
            for (int i = 0; i < to.Length && i < from.Length; i++)
            {
                to[i] = from[i];
            }
        }
    }
}

#endif