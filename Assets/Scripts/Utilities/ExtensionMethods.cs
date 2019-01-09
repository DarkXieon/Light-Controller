using System;
using System.Linq;

using LightControls.ControlOptions;

using LightControls.ControlOptions.ControlGroups;

using UnityEngine;

namespace LightControls.Utilities
{
    #region SerializedProperty Extensions

#if UNITY_EDITOR

    using UnityEditor;

    public static partial class ExtensionMethods
    {
        public static Gradient GetGradientValue(this SerializedProperty property)
        {
            return ReflectionUtils.GetMemberAtPath<Gradient>(property, "gradientValue");
        }

        public static void SetGradientValue(this SerializedProperty property, Gradient value)
        {
            ReflectionUtils.SetMemberAtPath(property, value, "gradientValue");
        }

        public static void SetArray<ArrayType>(this SerializedProperty arrayProperty, ArrayType[] array)
        {
            Debug.Assert(arrayProperty.isArray);

            SerializedProperty arrayLength = arrayProperty.FindPropertyRelative("Array.size");
            arrayLength.intValue = array.Length;

            for (int i = 0; i < array.Length; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);

                switch (element.propertyType)
                {
                    case SerializedPropertyType.AnimationCurve:
                        element.animationCurveValue = array[i] as AnimationCurve;
                        break;
                    case SerializedPropertyType.ArraySize:
                        element.intValue = (int)(object)array[i];
                        break;
                    case SerializedPropertyType.Boolean:
                        element.boolValue = (bool)(object)array[i];
                        break;
                    case SerializedPropertyType.Bounds:
                        element.boundsValue = (Bounds)(object)array[i];
                        break;
                    case SerializedPropertyType.BoundsInt:
                        element.boundsIntValue = (BoundsInt)(object)array[i];
                        break;
                    case SerializedPropertyType.Character:
                        element.intValue = (char)(object)array[i];
                        break;
                    case SerializedPropertyType.Color:
                        element.colorValue = (Color)(object)array[i];
                        break;
                    case SerializedPropertyType.Enum:
                        element.intValue = (int)(object)array[i];
                        break;
                    case SerializedPropertyType.ExposedReference:
                        element.exposedReferenceValue = array[i] as UnityEngine.Object;
                        break;
                    case SerializedPropertyType.FixedBufferSize:
                        element.intValue = (int)(object)array[i];
                        break;
                    case SerializedPropertyType.Float:
                        element.floatValue = (float)(object)array[i];
                        break;
                    case SerializedPropertyType.Gradient:
                        element.SetGradientValue((Gradient)(object)array[i]);
                        break;
                    case SerializedPropertyType.Integer:
                        element.intValue = (int)(object)array[i];
                        break;
                    case SerializedPropertyType.LayerMask:
                        element.intValue = (int)(object)array[i];
                        break;
                    case SerializedPropertyType.ObjectReference:
                        element.objectReferenceValue = array[i] as UnityEngine.Object;
                        break;
                    case SerializedPropertyType.Quaternion:
                        element.quaternionValue = (Quaternion)(object)array[i];
                        break;
                    case SerializedPropertyType.Rect:
                        element.rectValue = (Rect)(object)array[i];
                        break;
                    case SerializedPropertyType.RectInt:
                        element.rectIntValue = (RectInt)(object)array[i];
                        break;
                    case SerializedPropertyType.String:
                        element.stringValue = (string)(object)array[i];
                        break;
                    case SerializedPropertyType.Vector2:
                        element.vector2Value = (Vector2)(object)array[i];
                        break;
                    case SerializedPropertyType.Vector2Int:
                        element.vector2IntValue = (Vector2Int)(object)array[i];
                        break;
                    case SerializedPropertyType.Vector3:
                        element.vector3Value = (Vector3)(object)array[i];
                        break;
                    case SerializedPropertyType.Vector3Int:
                        element.vector3IntValue = (Vector3Int)(object)array[i];
                        break;
                    case SerializedPropertyType.Vector4:
                        element.vector4Value = (Vector4)(object)array[i];
                        break;
                    default:
                        Debug.LogError("Unrecognized type");
                        break;
                }
            }
        }

        public static ArrayType[] GetArray<ArrayType>(this SerializedProperty curveProperty)
        {
            Debug.Assert(curveProperty.isArray);

            object[] array = new object[curveProperty.arraySize];

            for (int i = 0; i < array.Length; i++)
            {
                SerializedProperty element = curveProperty.GetArrayElementAtIndex(i);

                if (typeof(ArrayType) == typeof(SerializedProperty))
                {
                    array[i] = element;

                    continue;
                }

                switch (element.propertyType)
                {
                    case SerializedPropertyType.AnimationCurve:
                        array[i] = element.animationCurveValue;
                        break;
                    case SerializedPropertyType.ArraySize:
                        array[i] = element.intValue;
                        break;
                    case SerializedPropertyType.Boolean:
                        array[i] = element.boolValue;
                        break;
                    case SerializedPropertyType.Bounds:
                        array[i] = element.boundsValue;
                        break;
                    case SerializedPropertyType.BoundsInt:
                        array[i] = element.boundsIntValue;
                        break;
                    case SerializedPropertyType.Character:
                        array[i] = element.intValue;
                        break;
                    case SerializedPropertyType.Color:
                        array[i] = element.colorValue;
                        break;
                    case SerializedPropertyType.Enum:
                        array[i] = element.intValue;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        array[i] = element.exposedReferenceValue;
                        break;
                    case SerializedPropertyType.FixedBufferSize:
                        array[i] = element.intValue;
                        break;
                    case SerializedPropertyType.Float:
                        array[i] = element.floatValue;
                        break;
                    case SerializedPropertyType.Gradient:
                        array[i] = element.GetGradientValue();
                        break;
                    case SerializedPropertyType.Integer:
                        array[i] = element.intValue;
                        break;
                    case SerializedPropertyType.LayerMask:
                        array[i] = element.intValue;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        array[i] = element.objectReferenceValue;
                        break;
                    case SerializedPropertyType.Quaternion:
                        array[i] = element.quaternionValue;
                        break;
                    case SerializedPropertyType.Rect:
                        array[i] = element.rectValue;
                        break;
                    case SerializedPropertyType.RectInt:
                        array[i] = element.rectIntValue;
                        break;
                    case SerializedPropertyType.String:
                        array[i] = element.stringValue;
                        break;
                    case SerializedPropertyType.Vector2:
                        array[i] = element.vector2Value;
                        break;
                    case SerializedPropertyType.Vector2Int:
                        array[i] = element.vector2IntValue;
                        break;
                    case SerializedPropertyType.Vector3:
                        array[i] = element.vector3Value;
                        break;
                    case SerializedPropertyType.Vector3Int:
                        array[i] = element.vector3IntValue;
                        break;
                    case SerializedPropertyType.Vector4:
                        array[i] = element.vector4Value;
                        break;
                    default:
                        Debug.LogError("Unrecognized type");
                        break;
                }
            }

            return array.Cast<ArrayType>().ToArray();
        }
    }

#endif

    #endregion

    #region AnimationCurve Extensions

    public static partial class ExtensionMethods
    {
        public static float GetTotalCycleTime(this AnimationCurve curve)
        {
            return curve[curve.length - 1].time - curve[0].time;
        }

        public static bool RescaleTime(this AnimationCurve curve, float totalCycleTime)
        {
            if (curve.length > 1)
            {
                var keys = curve.keys;
                float startTime = keys[0].time;
                float endTime = keys[keys.Length - 1].time;
                float totalTime = endTime - startTime;

                if (totalCycleTime != totalTime)
                {
                    float totalChange = totalTime - totalCycleTime;

                    for (int i = 0; i < curve.length; i++)
                    {
                        float time = keys[i].time;
                        float percentFromStart = (time - startTime) / totalTime;
                        float change = totalChange * percentFromStart;
                        float tangentMultiplier = totalTime / totalCycleTime;

                        keys[i].time = time - change;
                        keys[i].inTangent *= tangentMultiplier;
                        keys[i].outTangent *= tangentMultiplier;
                    }

                    curve.keys = keys;

                    return true;
                }
            }

            return false;
        }
    }

    #endregion
    
    #region Conversion Extensions

    public static partial class ExtensionMethods
    {
        public static WrapMode ConvertWrapMode(this MinMaxWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case MinMaxWrapMode.PingPong:
                    return WrapMode.PingPong;
                case MinMaxWrapMode.Loop:
                    return WrapMode.Loop;
                default:
                    return WrapMode.Clamp;
            }
        }

        public static MinMaxWrapMode ConvertWrapMode(this WrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case WrapMode.PingPong:
                    return MinMaxWrapMode.PingPong;
                case WrapMode.Loop:
                    return MinMaxWrapMode.Loop;
                default:
                    return MinMaxWrapMode.Clamp;
            }
        }

        public static IntensityControlTarget[] GetAllFlags(this IntensityControlTarget target)
        {
            string binaryString = Convert.ToString((int)target, 2);
            IntensityControlTarget[] controlTargets = new IntensityControlTarget[binaryString.Count(character => character == '1')]; //already set to the number of flags there will be

            if(controlTargets.Length > 0)
            {
                int index = 0;

                for (int i = 0; i < binaryString.Length; i++)
                {
                    if (binaryString[binaryString.Length - 1 - i] == '1') //The binary number will have the lowest values on the right and not the left
                    {
                        controlTargets[index] = (IntensityControlTarget)Mathf.Pow(2f, i);
                        index++;
                    }
                }
            }

            return controlTargets;
        }
    }

    #endregion
    
}