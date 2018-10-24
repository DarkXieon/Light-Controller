using LightControls.ControlOptions.ControlGroups.Data;

using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups
{
    public class CurveControlGroup
    {
        public bool UseControl => curveData.UseControl;
        public ParticleSystemCurveMode CurveMode => curveData.MinMaxCurve.mode;
        public float MaxValue => curveData.MaxValue;
        public float MinValue => curveData.MinValue;

        private CurveControlGroupData curveData;
        private MinMaxTracker minCurveTracker;
        private MinMaxTracker maxCurveTracker;
        private float currentValue;
        
        public CurveControlGroup(CurveControlGroupData data)
        {
            curveData = data;
            minCurveTracker = new MinMaxTracker(data.MinCurveTracker);
            maxCurveTracker = new MinMaxTracker(data.MaxCurveTracker);
            currentValue = GetCurrentValue();
        }
        
        #region Basic Control Methods

        public float GetControlValue()
        {
            //Debug.Assert(curveData.UseControl, "Getting control value for disabled control");
            
            return currentValue;
        }

        public bool IncrementControlValue()
        {
            return IncrementControlValue(Time.deltaTime);
        }

        public bool IncrementControlValue(float time)
        {
            if (curveData.MinMaxCurve.mode == ParticleSystemCurveMode.TwoCurves && maxCurveTracker.NormalTime != minCurveTracker.NormalTime)
            {
                minCurveTracker.NormalTime = maxCurveTracker.NormalTime;
            }

            bool finishedIteration = true;

            if(curveData.MinMaxCurve.mode == ParticleSystemCurveMode.Curve)
            {
                finishedIteration = maxCurveTracker.IncrementBy(time);
            }
            else if(curveData.MinMaxCurve.mode == ParticleSystemCurveMode.TwoCurves)
            {
                float maxCurveMax = Mathf.Abs(maxCurveTracker.GetCurrentPoint() - maxCurveTracker.CurrentTime);
                float minCurveMax = Mathf.Abs(minCurveTracker.GetCurrentPoint() - minCurveTracker.CurrentTime);

                time = Mathf.Min(time, maxCurveMax, minCurveMax);

                bool maxChanged = maxCurveTracker.IncrementBy(time);
                bool minChanged = minCurveTracker.IncrementBy(time);

                finishedIteration = maxCurveTracker.MinMaxPoints[maxCurveTracker.MinMaxPoints.Length - 1] > minCurveTracker.MinMaxPoints[minCurveTracker.MinMaxPoints.Length - 1]
                    ? maxChanged
                    : minChanged;
            }

            //for some reason using the minmaxcurve's evaluate method won't work
            currentValue = GetCurrentValue();

            return finishedIteration;
        }

        #endregion

        #region Evaluation Methods
        
        private float GetCurrentValue()
        {
            if (curveData.MinMaxCurve.mode == ParticleSystemCurveMode.TwoCurves)
            {
                var value1 = curveData.MinMaxCurve.curveMin.Evaluate(minCurveTracker.CurrentTime);
                var value2 = curveData.MinMaxCurve.curveMax.Evaluate(maxCurveTracker.CurrentTime);

                return RandomValueBetween(value1, value2) * curveData.MinMaxCurve.curveMultiplier;
            }
            else if (curveData.MinMaxCurve.mode == ParticleSystemCurveMode.Curve)
            {
                return curveData.MinMaxCurve.curve.Evaluate(maxCurveTracker.CurrentTime) * curveData.MinMaxCurve.curveMultiplier;
            }
            else if (curveData.MinMaxCurve.mode == ParticleSystemCurveMode.TwoConstants)
            {
                return RandomValueBetween(curveData.MinMaxCurve.constantMin, curveData.MinMaxCurve.constantMax);
            }
            else if (curveData.MinMaxCurve.mode == ParticleSystemCurveMode.Constant)
            {
                return curveData.MinMaxCurve.constant;
            }
            else
            {
                throw new System.Exception();
            }
        }
        
        public float GetCurrentMinMaxValue()
        {
            Debug.Assert(curveData.MinMaxCurve.mode == ParticleSystemCurveMode.Curve);
            
            return maxCurveTracker.CurrentIndex == maxCurveTracker.MinMaxPoints.Length - 1 && maxCurveTracker.WrapMode == MinMaxWrapMode.Loop
                ? curveData.MinMaxCurve.curveMax[curveData.MinMaxCurve.curveMax.length - 1].value * curveData.MinMaxCurve.curveMultiplier
                : curveData.MinMaxCurve.curve.Evaluate(maxCurveTracker.GetCurrentPoint()) * curveData.MinMaxCurve.curveMultiplier;
        }
        
        #endregion

        #region Curve Information Calculation

        public float? GetTotalCycleTime()
        {
            if (curveData.MinMaxCurve.mode == ParticleSystemCurveMode.Curve)
            {
                var totalTime = curveData.MinMaxCurve.curve[curveData.MinMaxCurve.curve.length - 1].time - curveData.MinMaxCurve.curve[0].time;

                return totalTime;
            }
            else if (curveData.MinMaxCurve.mode == ParticleSystemCurveMode.TwoCurves)
            {
                var totalTime1 = curveData.MinMaxCurve.curveMax[curveData.MinMaxCurve.curveMax.length - 1].time - curveData.MinMaxCurve.curveMax[0].time;
                var totalTime2 = curveData.MinMaxCurve.curveMin[curveData.MinMaxCurve.curveMin.length - 1].time - curveData.MinMaxCurve.curveMin[0].time;

                return Mathf.Max(totalTime1, totalTime2);
            }
            else
            {
                return null;
            }
        }
        
        #endregion
            
        #region Logic Performing Methods

        public bool IsIncreasing()
        {
            return GetCurrentMinMaxValue() > currentValue;
        }

        public bool IsCurveBased()
        {
            return curveData.IsCurveBased;
        }

        #endregion

        #region Misc

        private float RandomValueBetween(float value1, float value2)
        {
            var min = value1 > value2
                ? value2
                : value1;

            var max = min == value1
                ? value2
                : value1;

            var random = UnityEngine.Random.Range(min, max);

            return random;
        }

        #endregion
    }
}