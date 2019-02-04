using LightControls.Controllers;

using UnityEngine;

namespace Assets.Example
{
    public class StagerConditionsExample : MonoBehaviour
    {
        public bool ContinueIf(StageEventInfo dectectInfo)
        {
            return !Input.GetKeyDown(KeyCode.A);
        }

        public bool AdvanceIf(StageEventInfo dectectInfo)
        {
            return Input.GetKeyDown(KeyCode.A);
        }

        public bool OnDurationFinish(StageEventInfo dectectInfo)
        {
            return dectectInfo.IsDurationDone;
        }
    }
}
