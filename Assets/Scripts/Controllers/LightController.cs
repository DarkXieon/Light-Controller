using System;
using System.Linq;
using UnityEngine;

namespace LightControls.Controllers
{
    public abstract class LightController : MonoBehaviour
    {
        public abstract ILightControllerInfo[] LightControllerInfo { get; }

        [NonSerialized]
        public bool IsChild = false;

        protected int[] controlIterations = new int[0];
        protected int minIterations = 0;
        
        private void Update()
        {
            InternalUpdate();
            InternalApply();
        }
        
        public bool ExternalUpdate()
        {
            if(!IsChild)
            {
                Debug.LogWarning("Non childed light controller is being externally updated.");
            }

            return UpdateControls();
        }

        private bool InternalUpdate()
        {
            if(!IsChild)
            {
                return UpdateControls();
            }
            else
            {
                return false;
            }
        }

        protected bool UpdateControls()
        {
            if (controlIterations.Length != LightControllerInfo.Length)
            {
                Array.Resize(ref controlIterations, LightControllerInfo.Length);
            }

            for (int i = 0; i < LightControllerInfo.Length; i++)
            {
                bool iterated = LightControllerInfo[i].UpdateControls();

                if (iterated)
                    controlIterations[i]++;
            }

            if (controlIterations.Min() > minIterations)
            {
                minIterations = controlIterations.Min();

                return true;
            }

            return false;
        }

        public void ExternalApply()
        {
            if (!IsChild)
            {
                Debug.LogWarning("Non childed light controller is being externally applied. This means it is also being updated during the Update function.");
            }

            ApplyControls();
        }

        private void InternalApply()
        {
            if (!IsChild)
            {
                ApplyControls();
            }
        }

        private void ApplyControls()
        {
            for (int i = 0; i < LightControllerInfo.Length; i++)
            {
                LightControllerInfo[i].ApplyControls();
            }
        }
    }
}