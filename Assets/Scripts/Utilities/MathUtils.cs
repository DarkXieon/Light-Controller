using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace LightControls.Utilities
{
    public static class MathUtils
    {
        public static float ReMap(float current, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (current - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }
        
        public static float[] GetMinMaxTimes(AnimationCurve curve)
        {
            List<float> testList = new List<float>();

            for (int i = 1; i < curve.length; i++)
            {
                Keyframe keyframe0 = curve[i - 1];
                Keyframe keyframe1 = curve[i];
                
                Keyframe? next = i < curve.length - 1
                    ? curve[i + 1]
                    : (Keyframe?)null;
                
                float[] minMaxs = GetMinMaxTimes(keyframe0, keyframe1);

                if(minMaxs.Length == 0 && next.HasValue)
                {
                    minMaxs = GetMinMaxTimes(keyframe1, next.Value);

                    if(minMaxs.Length == 0)
                    {
                        bool isMiddleSignificant = IsDefinantSignificant(keyframe0, keyframe1, next.Value);

                        if(isMiddleSignificant)
                        {
                            testList.Add(keyframe1.time);
                        }
                    }
                }

                if (minMaxs.Length > 0)
                {
                    testList.AddRange(minMaxs);
                }
            }
            
            testList.Add(curve[0].time);
            testList.Add(curve[curve.length - 1].time);
            
            return testList
                .Distinct()
                //.Where(time => !testList
                    //.Any(otherTime => time != otherTime && Mathf.Approximately(time, otherTime)))
                .OrderBy(time => time)
                .ToArray();
        }
        
        public static float[] GetMinMaxTimes(Keyframe keyframe0, Keyframe keyframe1)
        {
            float dt = keyframe1.time - keyframe0.time;

            float p0 = keyframe0.value;
            float p1 = keyframe1.value;

            float m0 = keyframe0.outTangent * dt;
            float m1 = keyframe1.inTangent * dt;

            float a = 6 * p0 + 3 * m0 - 6 * p1 + 3 * m1;
            float b = -6 * p0 - 4 * m0 + 6 * p1 - 2 * m1;
            float c = m0;

            float discriminant = Mathf.Pow(b, 2) - 4 * a * c;

            if(discriminant >= 0 && a != 0)
            {
                float t1 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
                float t2 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);

                //This prevents rounding errors to mess up the results
                t1 = Mathf.Approximately(t1, 0f)
                    ? 0f
                    : Mathf.Approximately(t1, 1f)
                        ? 1f
                        : t1;

                //This prevents rounding errors to mess up the results
                t2 = Mathf.Approximately(t2, 0f)
                    ? 0f
                    : Mathf.Approximately(t2, 1f)
                        ? 1f
                        : t2;

                if (t1 > 0 && t1 < 1 && t2 > 0 && t2 < 1)
                {
                    return new float[] { t1 * dt + keyframe0.time, t2 * dt + keyframe0.time };
                }
                else if (t1 > 0 && t1 < 1)
                {
                    return new float[] { t1 * dt + keyframe0.time };
                }
                else if (t2 > 0 && t2 < 1)
                {
                    return new float[] { t2 * dt + keyframe0.time };
                }
                else
                {
                    return new float[0];
                }
            }
            else
            {
                return new float[0];
            }
        }
        
        private static bool IsDefinantSignificant(Keyframe keyframe0, Keyframe keyframe1, Keyframe keyframe2)
        {
            bool otherSignificant = (keyframe0.value < keyframe1.value && keyframe2.value < keyframe1.value) || (keyframe0.value > keyframe1.value && keyframe2.value > keyframe1.value) || (keyframe0.value == keyframe1.value || keyframe2.value == keyframe1.value); //maybe change that last statement to 0 and 1 not 1 and 2

            return otherSignificant;
        }
        
        public static float RandomBetweenTwoValues(float value1, float value2)
        {
            float max = Mathf.Max(value1, value2);
            float min = value1 == max
                ? value2
                : value1;

            return UnityEngine.Random.Range(min, max);
        }
    }
}
