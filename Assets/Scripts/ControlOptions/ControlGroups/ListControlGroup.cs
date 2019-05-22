using LightControls.ControlOptions.ControlGroups.Data;

using UnityEngine;

namespace LightControls.ControlOptions.ControlGroups
{
    public class ListControlGroup
    {
        public bool UseControl => listData.UseControl;
        public float[] ListedValues => listData.ListedValues;
        public float MinValue => listData.MinValue;
        public float MaxValue => listData.MaxValue;

        private ListControlGroupData listData;

        private float currentValue = 0f;
        private int currentEntry = 0;
        private int nextEntry = 0;
        
        public ListControlGroup(ListControlGroupData data)
        {
            listData = data;
            
            currentValue = listData.ListedValues[0];
            currentEntry = 0;
            nextEntry = FindNextIndex();
        }
        
        public float GetControlValue()
        {
            return currentValue;
        }

        public bool IncrementControlValue()
        {
            Debug.Assert(listData.UseControl, "Getting control value for disabled control");

            if (listData.ListedValues.Length > 1)
            {
                currentValue = listData.RandomizeBetweenValues
                        ? Utilities.MathUtils.RandomBetweenTwoValues(listData.ListedValues[currentEntry], listData.ListedValues[nextEntry])
                        : listData.ListedValues[nextEntry];

                //this might cause an unexpected return of true on the first call of this method. . . not sure. . . should look into that
                bool iterationDone = currentEntry >= nextEntry;

                currentEntry = nextEntry;

                nextEntry = FindNextIndex();

                return iterationDone;
            }
            else if(listData.ListedValues.Length == 1)
            {
                currentValue = listData.ListedValues[0];
            }

            return true;
        }
        
        private int FindNextIndex()
        {
            var nextIndex = currentEntry;

            if (listData.ListedValues.Length > 1)
            {
                while (nextIndex == currentEntry)
                {
                    nextIndex = listData.RandomizeChosenEntry && listData.ListedValues.Length > 2
                        ? Random.Range(0, listData.ListedValues.Length - 1)
                        : nextIndex + 1;

                    nextIndex = nextIndex < listData.ListedValues.Length
                        ? nextIndex
                        : 0;
                }
            }

            return nextIndex;
        }
    }
}