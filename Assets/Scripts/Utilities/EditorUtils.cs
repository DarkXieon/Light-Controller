#if UNITY_EDITOR

using System;
using System.Linq;
using LightControls.ControlOptions;
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
        //public static GUIStyle DefaultStyle;

        public static float LineHeight;

        public static float HorizontalBuffer;
        public static float IndentSpace;

        public static float VerticalBuffer;
        public static float VerticalSpace;

        public static string EditedValue;
        public static string EditedControl;

        private static int lineCount;
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            RichTextStyle = new GUIStyle() { richText = true, alignment = TextAnchor.MiddleLeft, fontSize = 14 };
            //DefaultStyle = new GUIStyle() { alignment = TextAnchor.MiddleLeft, fontSize = 14 }; //the font size should really be 11
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
                //EditorGUILayout.BeginHorizontal();
                EditedValue = EditorGUILayout.TextField(label, EditedValue, EditorStyles.numberField);
                //EditorGUILayout.EndHorizontal();

                // Parse number
                int number = 0;
                if (int.TryParse(EditedValue, out number))
                {
                    value = number;
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
                    value = number;
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

                    EditorGUI.PropertyField(currentRect, element, currentContent);

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
                        //GUIStyle otherStyle = new GUIStyle(EditorStyles.label);
                        //otherStyle.alignment = TextAnchor.MiddleRight;

                        SerializedProperty element = intensityModifiersProperty.GetArrayElementAtIndex(i);

                        SerializedProperty multiplier = element.FindPropertyRelative("Multiplier");
                        SerializedProperty offset = element.FindPropertyRelative("Offset");
                        SerializedProperty relateInversely = element.FindPropertyRelative("RelateInversely");

                        GUIContent contentAtIndex = intensityModifiersElementContent[i];

                        //float previousLabelWidth = EditorGUIUtility.labelWidth;
                        //float previousFieldWidth = EditorGUIUtility.fieldWidth;
                        //EditorGUIUtility.labelWidth = 0f;
                        //EditorGUIUtility.fieldWidth = 10f;
                        
                        Rect[] splitLine = SplitLineRect(
                            lineRect: currentRect,
                            labelContent: new GUIContent[] { contentAtIndex, intensityModifiersScaleContent, intensityModifiersOffsetContent, intensityModifiersRelateInverselyContent },
                            sizePercentages: new float[] { .4f, .2f, .2f, .2f },
                            labelStyle: EditorStyles.label,
                            linesDownward: 0);

                        EditorGUI.LabelField(splitLine[0], contentAtIndex);

                        EditorGUI.LabelField(splitLine[2], intensityModifiersScaleContent);
                        multiplier.floatValue = EditorGUI.FloatField(splitLine[3], multiplier.floatValue);

                        EditorGUI.LabelField(splitLine[4], intensityModifiersOffsetContent);
                        offset.floatValue = EditorGUI.FloatField(splitLine[5], offset.floatValue);

                        EditorGUI.LabelField(splitLine[6], intensityModifiersRelateInverselyContent);
                        relateInversely.boolValue = EditorGUI.Toggle(splitLine[7], relateInversely.boolValue);

                        currentRect = GetRectBelow(currentRect, EditorStyles.label);

                        //EditorGUILayout.BeginHorizontal();

                        //EditorGUILayout.LabelField(contentAtIndex, GUILayout.Width(previousLabelWidth - 5f));

                        //EditorGUILayout.LabelField(intensityModifiersScaleContent, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(EditorStyles.label.CalcSize(intensityModifiersScaleContent).x));
                        //EditorGUILayout.PropertyField(multiplier, GUIContent.none, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(minIntensityModifierFieldWidth));

                        //EditorGUILayout.LabelField(intensityModifiersOffsetContent, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(EditorStyles.label.CalcSize(intensityModifiersOffsetContent).x));
                        //EditorGUILayout.PropertyField(offset, GUIContent.none, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .15f), GUILayout.MinWidth(minIntensityModifierFieldWidth));

                        //EditorGUILayout.LabelField(intensityModifiersRelateInverselyContent, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .275f), GUILayout.MinWidth(EditorStyles.label.CalcSize(intensityModifiersRelateInverselyContent).x));
                        //EditorGUILayout.PropertyField(relateInversely, GUIContent.none, GUILayout.MaxWidth((EditorGUIUtility.currentViewWidth - previousLabelWidth - 5f) * .1f), GUILayout.MinWidth(10f));

                        //EditorGUILayout.EndHorizontal();

                        //EditorGUIUtility.labelWidth = previousLabelWidth;
                        //EditorGUIUtility.fieldWidth = previousFieldWidth;
                    }
                }

                //EditorGUILayout.Space();
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