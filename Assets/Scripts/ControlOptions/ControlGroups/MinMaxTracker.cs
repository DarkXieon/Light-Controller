using System;
using System.Linq;
using LightControls.ControlOptions.ControlGroups.Data;
using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups
{
    public enum IndexDirection
    {
        Forward = 1,
        Backward = -1
    }

    public enum MinMaxWrapMode
    {
        PingPong,
        Loop,
        Clamp
    }
    
    public class MinMaxTracker
    {
        private MinMaxTrackerData trackerData;

        public float[] MinMaxPoints => trackerData.MinMaxPoints;
        public MinMaxWrapMode WrapMode => trackerData.WrapMode;
        public float CurrentTime => currentTime;
        public int CurrentIndex => currentIndex;

        public float NormalTime
        {
            get
            {
                return indexDirection == IndexDirection.Backward
                    ? trackerData.MinMaxPoints[trackerData.MinMaxPoints.Length - 1] * 2 - currentTime
                    : currentTime;
            }
            set
            {
                switch (trackerData.WrapMode)
                {
                    case MinMaxWrapMode.Loop:
                        currentTime = Mathf.Repeat(value, trackerData.MinMaxPoints[trackerData.MinMaxPoints.Length - 1]) + trackerData.MinMaxPoints[0];
                        indexDirection = IndexDirection.Forward;
                        break;
                    case MinMaxWrapMode.PingPong:
                        currentTime = Mathf.PingPong(value, trackerData.MinMaxPoints[trackerData.MinMaxPoints.Length - 1]) + trackerData.MinMaxPoints[0];
                        indexDirection = value >= trackerData.MinMaxPoints[trackerData.MinMaxPoints.Length - 1]
                            ? IndexDirection.Backward
                            : IndexDirection.Forward;
                        break;
                    default:
                        currentTime = Mathf.Clamp(value, 0, trackerData.MinMaxPoints[trackerData.MinMaxPoints.Length - 1]) + trackerData.MinMaxPoints[0];
                        indexDirection = IndexDirection.Forward;
                        break;
                }

                currentIndex = indexDirection == IndexDirection.Forward
                    ? Array.IndexOf(trackerData.MinMaxPoints, trackerData.MinMaxPoints.First(time1 => time1 > currentTime))
                    : Array.IndexOf(trackerData.MinMaxPoints, trackerData.MinMaxPoints.Last(time1 => time1 < currentTime));

                if (value <= trackerData.MinMaxPoints[0])
                {
                    currentIndex = 1;
                }
                else if (indexDirection == IndexDirection.Forward && currentIndex == 0 && value > trackerData.MinMaxPoints[0])
                {
                    currentIndex = trackerData.MinMaxPoints.Length - 1;
                }
            }
        }
        
        private int currentIndex;
        private int iterations;
        private float currentTime;
        private IndexDirection indexDirection = IndexDirection.Forward;

        public MinMaxTracker(MinMaxTrackerData data)
        {
            trackerData = data;

            currentIndex = 0;
            iterations = 0;
            currentTime = 0f;
            indexDirection = IndexDirection.Forward;
        }
        
        public float GetCurrentPoint()
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, trackerData.MinMaxPoints.Length - 1); //This is so when the index gets updated from the UpdateSerialized method in ReflectionUtils or if the trackerData.MinMaxPoints are set from somewhere else, it is always within the correct bounds

            return trackerData.MinMaxPoints[currentIndex];
        }

        public bool IncrementBy(float time)
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, trackerData.MinMaxPoints.Length - 1); //This is so when the index gets updated from the UpdateSerialized method in ReflectionUtils or if the trackerData.MinMaxPoints are set from somewhere else, it is always within the correct bounds

            if (indexDirection == IndexDirection.Forward)
            {
                currentTime = Mathf.Clamp(currentTime + time, trackerData.MinMaxPoints[0], trackerData.MinMaxPoints[currentIndex]);

                if (currentTime >= trackerData.MinMaxPoints[currentIndex]) //NOTE: ADD A MATHF.APPROXOMATLY FUNTION HERE IF THINGS ACT WEIRD
                {
                    return IncrementPointIndex();
                }
            }
            else
            {
                currentTime = Mathf.Clamp(currentTime - time, trackerData.MinMaxPoints[currentIndex], trackerData.MinMaxPoints[trackerData.MinMaxPoints.Length - 1]);

                if (currentTime <= trackerData.MinMaxPoints[currentIndex])
                {
                    return IncrementPointIndex();
                }
            }

            return false;
        }
        
        private bool IncrementPointIndex()
        {
            int nextIndex = GetNextMinMaxCurveIndex();

            IndexDirection nextDirection = nextIndex > currentIndex || trackerData.WrapMode != MinMaxWrapMode.PingPong
                ? IndexDirection.Forward
                : IndexDirection.Backward;

            //bool completedIteration = (trackerData.WrapMode == MinMaxWrapMode.Loop && nextIndex < currentIndex)
            //                       || (trackerData.WrapMode == MinMaxWrapMode.Clamp && nextIndex == currentIndex && iterations < 1)
            //                       || (trackerData.WrapMode == MinMaxWrapMode.PingPong && indexDirection == IndexDirection.Backward && nextDirection == IndexDirection.Forward);

            bool completedIteration = (trackerData.WrapMode == MinMaxWrapMode.Loop && nextIndex < currentIndex)
                                   || (trackerData.WrapMode == MinMaxWrapMode.Clamp || trackerData.WrapMode == MinMaxWrapMode.PingPong && nextIndex == 0);

            if (completedIteration)
                iterations++;

            currentIndex = nextIndex;
            indexDirection = nextDirection;

            return completedIteration;
        }

        private int GetNextMinMaxCurveIndex()
        {
            int maxIndex = trackerData.MinMaxPoints.Length - 1;
            
            switch (trackerData.WrapMode)
            {
                case MinMaxWrapMode.Loop:
                    return Mathf.RoundToInt(Mathf.Repeat(currentIndex + 1, maxIndex + 1));
                case MinMaxWrapMode.PingPong:
                    int change = (int)indexDirection;
                    return Mathf.RoundToInt(Mathf.PingPong(currentIndex + change, maxIndex));
                default:
                    return Mathf.Clamp(currentIndex + 1, 0, maxIndex);
            }
        }
    }
}