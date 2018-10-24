using LightControls.Controllers;
using UnityEngine;

namespace LightControls.Utilities
{
    public static partial class StageEventHandlers
    {
        public static bool OnDurationFinish(StageEventInfo dectectInfo)
        {
            return dectectInfo.IsDurationDone;
        }

        public static bool ContinueIf(StageEventInfo dectectInfo)
        {
            return !Input.GetKeyDown(KeyCode.A);
        }

        public static bool AdvanceIf(StageEventInfo dectectInfo)
        {
            return Input.GetKeyDown(KeyCode.A);
        }
    }
}
