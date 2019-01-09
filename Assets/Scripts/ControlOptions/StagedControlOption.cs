using System.Linq;
using LightControls.ControlOptions.Stages;
using LightControls.Utilities;
using UnityEngine;

namespace LightControls.ControlOptions
{
    public enum StagingOptions
    {
        Time,
        Iterations,
        Event,
        None
    }

    [CreateAssetMenu(menuName = "Light Controller/Staged Control Option")]
    public class StagedControlOption : LightControlOption
    {
        public ControlOptionStager Stager;

        private void OnEnable()
        {
            Stager = Stager ?? new ControlOptionStager();
        }
        
        public override InstancedControlOption GetInstanced()
        {
            return new InstancedStagedControlOption(this);
        }
    }
}
